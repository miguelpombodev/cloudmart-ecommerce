using Identity.Infrastructure.Persistence;
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
}
