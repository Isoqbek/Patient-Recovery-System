using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RehabilitationService.Data;
using RehabilitationService.Services;
using Serilog;
using System.Reflection;
using FluentValidation.AspNetCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up Rehabilitation Service");

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
    builder.Services.AddDbContext<RehabilitationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add AutoMapper
    builder.Services.AddAutoMapper(typeof(Program));

    // Add Application Services
    builder.Services.AddScoped<IRehabilitationService, RehabilitationServices>();

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
        options.AddPolicy("RehabilitationReadPolicy", policy =>
            policy.RequireScope("rehabilitation.read"));

        options.AddPolicy("RehabilitationWritePolicy", policy =>
            policy.RequireScope("rehabilitation.write"));
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
        c.SwaggerDoc("v1", new() { Title = "Rehabilitation Service API", Version = "v1" });

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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rehabilitation Service API V1");
        });
    }

    app.UseSerilogRequestLogging();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Add health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "RehabilitationService" }));

    // Ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<RehabilitationDbContext>();
        context.Database.EnsureCreated();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Rehabilitation Service terminated unexpectedly");
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
        CreateMap<RehabilitationService.Models.RehabilitationPlan, RehabilitationService.DTOs.RehabilitationPlanDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PlanType, opt => opt.MapFrom(src => src.PlanType.ToString()))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty.ToString()))
            .ForMember(dest => dest.FormattedStartDate, opt => opt.MapFrom(src => src.StartDate.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.FormattedEndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToString("yyyy-MM-dd") : null))
            .ForMember(dest => dest.DaysActive, opt => opt.MapFrom(src => (DateTime.UtcNow - src.StartDate).Days))
            .ForMember(dest => dest.ProgressLogCount, opt => opt.MapFrom(src => src.ProgressLogs.Count))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == RehabilitationService.Models.PlanStatus.Active))
            .ForMember(dest => dest.CompletionPercentage, opt => opt.MapFrom(src => CalculateCompletionPercentage(src)))
            .ForMember(dest => dest.RecentProgressLogs, opt => opt.MapFrom(src => src.ProgressLogs.OrderByDescending(log => log.LogDate).Take(5)));

        CreateMap<RehabilitationService.DTOs.CreateRehabilitationPlanDto, RehabilitationService.Models.RehabilitationPlan>();
        CreateMap<RehabilitationService.DTOs.UpdateRehabilitationPlanDto, RehabilitationService.Models.RehabilitationPlan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PatientId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ProgressLogs, opt => opt.Ignore());

        CreateMap<RehabilitationService.Models.RehabilitationProgressLog, RehabilitationService.DTOs.ProgressLogDto>()
            .ForMember(dest => dest.ProgressType, opt => opt.MapFrom(src => src.ProgressType.ToString()))
            .ForMember(dest => dest.CompletionStatus, opt => opt.MapFrom(src => src.CompletionStatus.ToString()))
            .ForMember(dest => dest.FormattedLogDate, opt => opt.MapFrom(src => src.LogDate.ToString("yyyy-MM-dd HH:mm")))
            .ForMember(dest => dest.FormattedDuration, opt => opt.MapFrom(src => FormatDuration(src.DurationMinutes)))
            .ForMember(dest => dest.PainLevelDescription, opt => opt.MapFrom(src => GetLevelDescription(src.PainLevel, "Pain")))
            .ForMember(dest => dest.EnergyLevelDescription, opt => opt.MapFrom(src => GetLevelDescription(src.EnergyLevel, "Energy")))
            .ForMember(dest => dest.MoodLevelDescription, opt => opt.MapFrom(src => GetLevelDescription(src.MoodLevel, "Mood")));

        CreateMap<RehabilitationService.DTOs.CreateProgressLogDto, RehabilitationService.Models.RehabilitationProgressLog>();
    }

    private static double CalculateCompletionPercentage(RehabilitationService.Models.RehabilitationPlan plan)
    {
        if (plan.Status == RehabilitationService.Models.PlanStatus.Completed)
            return 100.0;

        if (plan.Status != RehabilitationService.Models.PlanStatus.Active)
            return 0.0;

        var totalDays = plan.EndDate.HasValue ? (plan.EndDate.Value - plan.StartDate).Days : plan.EstimatedDurationWeeks * 7;
        var elapsedDays = (DateTime.UtcNow - plan.StartDate).Days;

        if (totalDays <= 0) return 0.0;

        var percentage = Math.Min((double)elapsedDays / totalDays * 100, 100.0);
        return Math.Round(percentage, 1);
    }

    private static string FormatDuration(int? durationMinutes)
    {
        if (!durationMinutes.HasValue) return "";

        var hours = durationMinutes.Value / 60;
        var minutes = durationMinutes.Value % 60;

        if (hours > 0)
            return $"{hours}h {minutes}m";

        return $"{minutes}m";
    }

    private static string GetLevelDescription(int? level, string type)
    {
        if (!level.HasValue) return "";

        return type switch
        {
            "Pain" => level.Value switch
            {
                1 => "Very Low",
                2 => "Low",
                3 => "Mild",
                4 => "Mild-Moderate",
                5 => "Moderate",
                6 => "Moderate-High",
                7 => "High",
                8 => "Very High",
                9 => "Severe",
                10 => "Extreme",
                _ => level.Value.ToString()
            },
            "Energy" or "Mood" => level.Value switch
            {
                1 => "Very Low",
                2 => "Low",
                3 => "Below Average",
                4 => "Slightly Low",
                5 => "Average",
                6 => "Good",
                7 => "Very Good",
                8 => "High",
                9 => "Very High",
                10 => "Excellent",
                _ => level.Value.ToString()
            },
            _ => level.Value.ToString()
        };
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