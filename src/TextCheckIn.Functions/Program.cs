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
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http.Headers;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(workerApp =>
    {
        workerApp.UseMiddleware<SessionMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables()
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddUserSecrets(typeof(Program).Assembly, optional: true);
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddApplicationInsights();

            if (context.HostingEnvironment.IsDevelopment())
            {
                builder.SetMinimumLevel(LogLevel.Information);
            }
        });

        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ICheckInServiceRepository, CheckInServiceRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomersVehicleRepository, CustomersVehicleRepository>();

        services.AddScoped<MileageBucketService>();

        services.Configure<OmniXConfiguration>(
            configuration.GetSection(OmniXConfiguration.SectionName));

        services.Configure<SessionConfiguration>(
            configuration.GetSection(SessionConfiguration.SectionName));

        services.AddScoped<OmniXWebhookSignatureValidator>();

        var omniXConfig = configuration.GetSection(OmniXConfiguration.SectionName).Get<OmniXConfiguration>();

        services.AddScoped<IOmniXService, OmniXService>();

        var sessionConfig = configuration.GetSection(SessionConfiguration.SectionName).Get<SessionConfiguration>();

        if (sessionConfig?.StorageType == SessionStorageType.Redis)
        {
            services.AddSingleton<IRedisService, RedisService>();
            services.AddScoped<ISessionLoginService, RedisSessionLoginService>();
            services.AddLogging(builder => builder.AddFilter("TextCheckIn.Core.Services.RedisSessionLoginService", LogLevel.Debug));
        }
        else
        {
            services.AddScoped<ISessionLoginService, DatabaseSessionLoginService>();
            services.AddLogging(builder => builder.AddFilter("TextCheckIn.Core.Services.DatabaseSessionLoginService", LogLevel.Debug));
        }

        services.AddScoped<ICheckInSessionService, CheckInSessionService>();
        services.AddScoped<ISessionManagementService, SessionManagementService>();

        services.AddHttpClient("OmniXApi", client =>
        {
            if (omniXConfig?.ApiUrl != null)
            {
                client.BaseAddress = new Uri(omniXConfig.ApiUrl);
            }

            client.Timeout = TimeSpan.FromSeconds(omniXConfig?.TimeoutSeconds ?? 30);

            if (!string.IsNullOrEmpty(omniXConfig?.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", omniXConfig.ApiKey);
            }
        });

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

        services.Configure<SmsConfiguration>(
            configuration.GetSection(SmsConfiguration.SectionName));

        services.AddHttpClient<ISmsService, SmsService>();

        services.AddScoped<IOtpService, OtpService>();

        // Add Health Checks
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var healthChecksBuilder = services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"));

        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecksBuilder.AddSqlServer(
                connectionString,
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "sqlserver" });
        }
    })
    .Build();

host.Run();