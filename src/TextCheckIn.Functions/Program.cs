using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Core.Services;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Functions.Extensions;
using TextCheckIn.Core.Helpers;
using TextCheckIn.Data.Context;
using Microsoft.EntityFrameworkCore;
using TextCheckIn.Data.Repositories;
using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Functions.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(workerApp =>
    {
        // Add our session middleware to the worker pipeline
        workerApp.UseMiddleware<SessionMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        // Add configuration sources
        config.AddEnvironmentVariables()
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddUserSecrets(typeof(Program).Assembly, optional: true);
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Configure JSON serialization to use camelCase for all objects
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        // Configure Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddApplicationInsights();

            // Set minimum log level for development
            if (context.HostingEnvironment.IsDevelopment())
            {
                builder.SetMinimumLevel(LogLevel.Information);
            }
        });

        // Add Database context
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });

        // Register repositories
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ICheckInServiceRepository, CheckInServiceRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomersVehicleRepository, CustomersVehicleRepository>();

        // Configure omniX integration
        services.Configure<OmniXConfiguration>(
            configuration.GetSection(OmniXConfiguration.SectionName));

        // Configure Session Configuration
        services.Configure<SessionConfiguration>(
            configuration.GetSection(SessionConfiguration.SectionName));

        // Register OmniXWebhookSignatureValidator - now using standard DI resolution with IOptions
        services.AddScoped<OmniXWebhookSignatureValidator>();

        // Register services based on configuration
        var omniXConfig = configuration.GetSection(OmniXConfiguration.SectionName).Get<OmniXConfiguration>();

        services.AddScoped<OmniXServiceBase, OmniXService>();

        // Register session login services based on configuration
        var sessionConfig = configuration.GetSection(SessionConfiguration.SectionName).Get<SessionConfiguration>();

        if (sessionConfig?.StorageType == SessionStorageType.Redis)
        {
            // Use Redis for session storage
            services.AddSingleton<IRedisService, RedisService>();
            services.AddScoped<ISessionLoginService, RedisSessionLoginService>();
            services.AddLogging(builder => builder.AddFilter("TextCheckIn.Core.Services.RedisSessionLoginService", LogLevel.Debug));
        }
        else
        {
            // Use Database for session storage (default)
            services.AddScoped<ISessionLoginService, DatabaseSessionLoginService>();
            services.AddLogging(builder => builder.AddFilter("TextCheckIn.Core.Services.DatabaseSessionLoginService", LogLevel.Debug));
        }

        services.AddScoped<ICheckInSessionService, CheckInSessionService>();
        services.AddScoped<ISessionManagementService, SessionManagementService>(); // Add session management service

        // Configure HTTP client for external API calls (when real omniX service is implemented)
        services.AddHttpClient("OmniXApi", client =>
        {
            if (omniXConfig?.ApiUrl != null)
            {
                client.BaseAddress = new Uri(omniXConfig.ApiUrl);
            }

            client.Timeout = TimeSpan.FromSeconds(omniXConfig?.TimeoutSeconds ?? 30);

            // Add API key header when available
            if (!string.IsNullOrEmpty(omniXConfig?.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", omniXConfig.ApiKey);
            }
        });

        // Add CORS configuration (for development)
        if (context.HostingEnvironment.IsDevelopment())
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
        }

        // Configure SMS service
        services.Configure<SmsConfiguration>(
            configuration.GetSection(SmsConfiguration.SectionName));

        // Register SMS service with HttpClient
        services.AddHttpClient<ISmsService, SmsService>();

        // Register OTP service
        services.AddScoped<IOtpService, OtpService>();

        // Add health checks (commented out for now due to dependency issues)
        // services.AddHealthChecks()
        //         .AddCheck<OmniXHealthCheck>("omnix");
    })
    .Build();

host.Run();
