using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    string checkConnectionString =
      configuration.GetConnectionString("Database") ??
      throw new InvalidOperationException("No Connection String informed");

    configuration.GetConnectionString(checkConnectionString);

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

    services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(serviceName))
      .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter())
      .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

    return services;
  }
}
