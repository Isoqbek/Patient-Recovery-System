using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using KnowledgeBaseService.Data;
using KnowledgeBaseService.Services;
using Serilog;
using System.Reflection;
using FluentValidation.AspNetCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up Knowledge Base Service");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // Add services to the container
    builder.Services.AddControllers()
        .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()))
        .AddNewtonsoftJson();

    // Add Database
    builder.Services.AddDbContext<KnowledgeBaseDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add AutoMapper
    builder.Services.AddAutoMapper(typeof(Program));

    // Add Application Services
    builder.Services.AddScoped<IKnowledgeBaseService, KnowledgeBaseServices>();

    // Add Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["IdentityServer:Authority"];
            options.TokenValidationParameters.ValidateAudience = false;
            options.RequireHttpsMetadata = false; // Only for development
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidateLifetime = true;
            options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);

            // Debug logging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Error("Authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Log.Information("Token validated for user: {User}", context.Principal?.Identity?.Name ?? "Unknown");
                    return Task.CompletedTask;
                }
            };
        });

    // Add Authorization
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("KnowledgeReadPolicy", policy =>
            policy.RequireScope("knowledge.read"));

        options.AddPolicy("KnowledgeWritePolicy", policy =>
            policy.RequireScope("knowledge.write"));
    });

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Knowledge Base Service API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Knowledge Base Service API V1");
        });
    }

    app.UseSerilogRequestLogging();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Add health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "KnowledgeBaseService" }));

    // Ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<KnowledgeBaseDbContext>();
        context.Database.EnsureCreated();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Knowledge Base Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// AutoMapper Profile
public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        CreateMap<KnowledgeBaseService.Models.KnowledgeArticle, KnowledgeBaseService.DTOs.KnowledgeArticleDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.FormattedCreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd HH:mm")))
            .ForMember(dest => dest.FormattedLastReviewed, opt => opt.MapFrom(src => src.LastReviewedDate.HasValue ? src.LastReviewedDate.Value.ToString("yyyy-MM-dd") : null))
            .ForMember(dest => dest.FormattedNextReview, opt => opt.MapFrom(src => src.NextReviewDate.HasValue ? src.NextReviewDate.Value.ToString("yyyy-MM-dd") : null))
            .ForMember(dest => dest.NeedsReview, opt => opt.MapFrom(src => src.NextReviewDate.HasValue && src.NextReviewDate.Value <= DateTime.UtcNow))
            .ForMember(dest => dest.ContentPreview, opt => opt.MapFrom(src => CreateContentPreview(src.Content)))
            .ForMember(dest => dest.KeywordList, opt => opt.MapFrom(src => ParseKeywords(src.Keywords)));

        CreateMap<KnowledgeBaseService.DTOs.CreateKnowledgeArticleDto, KnowledgeBaseService.Models.KnowledgeArticle>();
        CreateMap<KnowledgeBaseService.DTOs.UpdateKnowledgeArticleDto, KnowledgeBaseService.Models.KnowledgeArticle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }

    private static string CreateContentPreview(string content)
    {
        if (string.IsNullOrEmpty(content))
            return "";

        // Remove markdown headers and formatting
        var cleanContent = content
            .Replace("#", "")
            .Replace("*", "")
            .Replace("**", "")
            .Replace("_", "")
            .Replace("`", "");

        // Take first 200 characters
        if (cleanContent.Length <= 200)
            return cleanContent.Trim();

        var preview = cleanContent.Substring(0, 200);
        var lastSpace = preview.LastIndexOf(' ');
        if (lastSpace > 150)
            preview = preview.Substring(0, lastSpace);

        return preview.Trim() + "...";
    }

    private static List<string> ParseKeywords(string? keywords)
    {
        if (string.IsNullOrEmpty(keywords))
            return new List<string>();

        return keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToList();
    }
}

// Extension method for scope requirement
public static class AuthorizationExtensions
{
    public static void RequireScope(this Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder policy, string scope)
    {
        policy.RequireClaim("scope", scope);
    }
}