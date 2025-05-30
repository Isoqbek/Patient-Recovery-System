using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MonitoringService.Data;
using MonitoringService.Services;
using Serilog;
using System.Reflection;
using FluentValidation.AspNetCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up Monitoring Service");

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
    builder.Services.AddDbContext<MonitoringDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add AutoMapper
    builder.Services.AddAutoMapper(typeof(Program));

    // Add HttpClient for external service calls - TUZATILDI
    builder.Services.AddHttpClient();

    // Add Application Services
    builder.Services.AddScoped<IAlertService, AlertService>();
    builder.Services.AddScoped<IClinicalDataMonitoringService, ClinicalDataMonitoringService>();





    builder.Services.AddSingleton<IEventBusService, RabbitMQEventBusService>();
    //builder.Services.AddSingleton<IEventBusService, MockEventBusService>();




    // Add Background Service
    builder.Services.AddHostedService<MonitoringBackgroundService>();

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
        options.AddPolicy("MonitoringReadPolicy", policy =>
            policy.RequireScope("monitoring.read"));

        options.AddPolicy("MonitoringWritePolicy", policy =>
            policy.RequireScope("monitoring.write"));
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
        c.SwaggerDoc("v1", new() { Title = "Monitoring Service API", Version = "v1" });

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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Monitoring Service API V1");
        });
    }

    app.UseSerilogRequestLogging();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Add health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "MonitoringService" }));

    // Ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MonitoringDbContext>();
        context.Database.EnsureCreated();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Monitoring Service terminated unexpectedly");
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
        CreateMap<MonitoringService.Models.Alert, MonitoringService.DTOs.AlertDto>()
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.FormattedAlertDateTime, opt => opt.MapFrom(src => src.AlertDateTime.ToString("yyyy-MM-dd HH:mm:ss")))
            .ForMember(dest => dest.TimeSinceCreated, opt => opt.MapFrom(src => CalculateTimeSince(src.CreatedAt)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => IsAlertActive(src.Status)));

        CreateMap<MonitoringService.DTOs.CreateAlertDto, MonitoringService.Models.Alert>();

        CreateMap<MonitoringService.DTOs.UpdateAlertDto, MonitoringService.Models.Alert>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PatientId, opt => opt.Ignore())
            .ForMember(dest => dest.AlertDateTime, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore())
            .ForMember(dest => dest.Severity, opt => opt.Ignore())
            .ForMember(dest => dest.TriggeringClinicalEntryId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }

    private static string CalculateTimeSince(DateTime createdAt)
    {
        var timeSpan = DateTime.UtcNow - createdAt;

        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} day(s) ago";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} hour(s) ago";
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} minute(s) ago";

        return "Just now";
    }

    private static bool IsAlertActive(MonitoringService.Models.AlertStatus status)
    {
        return status == MonitoringService.Models.AlertStatus.New ||
               status == MonitoringService.Models.AlertStatus.Acknowledged ||
               status == MonitoringService.Models.AlertStatus.InProgress;
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