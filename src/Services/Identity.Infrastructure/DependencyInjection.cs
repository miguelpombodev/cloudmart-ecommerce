using BuildingBlocks.Infrastructure;
using BuildingBlocks.Logging;
using Identity.Application.Abstractions;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Grafana.Loki;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    string connectionString =
      configuration.GetConnectionString("Database") ??
      throw new InvalidOperationException("No Connection String informed");

    bool isDevelopment = configuration["ASPNETCORE_ENVIRONMENT"] == "Development";

    var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
    {
      SslMode = isDevelopment ? SslMode.Disable : SslMode.Require, // TLS required in PRD
      Pooling = true,
      MinPoolSize = 1,
      MaxPoolSize = 20,
      Timeout = 15,
      CommandTimeout = 30,
      KeepAlive = 30, // Keep TCP connection alive through load balancers
      ApplicationName = "Catalog.Api"
    };

    services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    {
      options.UseNpgsql(connectionStringBuilder.ConnectionString, pgAction =>
      {
        pgAction.EnableRetryOnFailure(
          3,
          TimeSpan.FromSeconds(5),
          null);

        pgAction.CommandTimeout(30);
        pgAction.MigrationsHistoryTable("__ef_migrations_history");
      });

      options.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
      options.EnableDetailedErrors(isDevelopment);
      options.EnableSensitiveDataLogging(isDevelopment);
    });


    return services;
  }

  public static ConfigureHostBuilder AddHostBuilder(this ConfigureHostBuilder host)
  {
    host.UseSerilog((context, services, loggerConfig) =>
    {
      loggerConfig
        .ReadFrom
        .Configuration(context.Configuration)
        .Enrich
        .FromLogContext()
        .Enrich
        .WithMachineName()
        .Enrich
        .WithEnvironmentName()
        .Enrich
        .WithThreadId()
        .Enrich
        .WithProperty("ServiceName", "Identity.Api")
        .Enrich
        .With<TraceIdEnricher>()
        .WriteTo
        .Console(new RenderedCompactJsonFormatter())
        .WriteTo
        .GrafanaLoki(
          context.Configuration["Loki:Url"]!,
          new[] { new LokiLabel { Key = "service", Value = "identity-api" } });
    });

    return host;
  }

  public static ILoggingBuilder AddLoggingBuilder(
    this ILoggingBuilder logging,
    IConfiguration configuration)
  {
    string serviceName =
      configuration["ServiceName"] ?? throw new InvalidOperationException("No Service Name informed");

    Log.Logger = new LoggerConfiguration()
      .Enrich
      .WithProperty("service", serviceName)
      .Enrich
      .WithEnvironmentName()
      .Enrich
      .WithMachineName()
      .Enrich
      .WithThreadId()
      .Enrich
      .WithOpenTelemetryTraceId()
      .Enrich
      .WithOpenTelemetrySpanId()
      .WriteTo
      .Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{MachineName}] {Message:lj}{NewLine}{Exception}")
      .CreateLogger();

    logging.AddOpenTelemetry(options =>
    {
      options.SetResourceBuilder(
          ResourceBuilder.CreateDefault()
            .AddService(serviceName))
        .AddConsoleExporter();
    });

    return logging;
  }

  public static IServiceCollection AddTelemetryServices(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    string serviceName =
      configuration["ServiceName"] ?? throw new InvalidOperationException("No Service Name informed");

    string tempoUrl = configuration["GrafanaTempoUrl"] ??
                      throw new InvalidOperationException("Grafana Tempo URL not informed");

    services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(serviceName))
      .WithTracing(tracing => tracing
        .AddNpgsql()
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(opts => opts.Endpoint = new Uri(tempoUrl)))
      .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(opts => opts.Endpoint = new Uri(tempoUrl)));

    return services;
  }

  public static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
  }
}
