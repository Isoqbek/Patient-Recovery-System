using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ClinicalRecordService.Data;
using ClinicalRecordService.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Reflection;
using FluentValidation.AspNetCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up Clinical Record Service");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // Add services to the container
    builder.Services.AddControllers()
        .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()))
        .AddNewtonsoftJson(); // For proper JSON handling

    // Add Database
    builder.Services.AddDbContext<ClinicalRecordDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add AutoMapper
    builder.Services.AddAutoMapper(typeof(Program));

    // Add Application Services
    builder.Services.AddScoped<IClinicalRecordService, ClinicalRecordServices>();

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
        options.AddPolicy("ClinicalRecordReadPolicy", policy =>
            policy.RequireScope("clinicalrecord.read"));

        options.AddPolicy("ClinicalRecordWritePolicy", policy =>
            policy.RequireScope("clinicalrecord.write"));
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
        c.SwaggerDoc("v1", new() { Title = "Clinical Record Service API", Version = "v1" });

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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinical Record Service API V1");
        });
    }

    app.UseSerilogRequestLogging();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Add health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ClinicalRecordService" }));

    // Ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ClinicalRecordDbContext>();
        context.Database.EnsureCreated();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Clinical Record Service terminated unexpectedly");
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
        CreateMap<ClinicalRecordService.Models.ClinicalEntry, ClinicalRecordService.DTOs.ClinicalEntryDto>()
            .ForMember(dest => dest.EntryType, opt => opt.MapFrom(src => src.EntryType.ToString()))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => ParseJsonData(src.Data)))
            .ForMember(dest => dest.FormattedDateTime, opt => opt.MapFrom(src => src.EntryDateTime.ToString("yyyy-MM-dd HH:mm:ss")))
            .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => CreateSummary(src)));

        CreateMap<ClinicalRecordService.DTOs.CreateClinicalEntryDto, ClinicalRecordService.Models.ClinicalEntry>()
            .ForMember(dest => dest.Data, opt => opt.Ignore()); // Handled in service

        CreateMap<ClinicalRecordService.DTOs.UpdateClinicalEntryDto, ClinicalRecordService.Models.ClinicalEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PatientId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Data, opt => opt.Ignore()); // Handled in service
    }

    private static object? ParseJsonData(string? jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
            return null;

        try
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
        }
        catch
        {
            return jsonData; // Return as string if parsing fails
        }
    }

    private static string CreateSummary(ClinicalRecordService.Models.ClinicalEntry entry)
    {
        if (!string.IsNullOrEmpty(entry.Notes))
        {
            return entry.Notes.Length > 100 ? entry.Notes[..100] + "..." : entry.Notes;
        }

        return entry.EntryType switch
        {
            ClinicalRecordService.Models.EntryType.VitalSign => "Vital signs recorded",
            ClinicalRecordService.Models.EntryType.Symptom => "Symptom reported",
            ClinicalRecordService.Models.EntryType.Medication => "Medication administered",
            ClinicalRecordService.Models.EntryType.TestResult => "Test results available",
            ClinicalRecordService.Models.EntryType.Diagnosis => "Diagnosis recorded",
            ClinicalRecordService.Models.EntryType.Treatment => "Treatment administered",
            _ => $"{entry.EntryType} entry"
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