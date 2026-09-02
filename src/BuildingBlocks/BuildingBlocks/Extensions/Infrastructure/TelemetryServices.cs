using System.Reflection;
using BuildingBlocks.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class TelemetryServices
{
  public static readonly string OtelSectionName = "OpenTelemetry";

  public static IServiceCollection AddTelemetryProviderConfiguration(this IServiceCollection services, IConfiguration configuration)
  {
    OpenTelemetryOptions openTelemetryOptions = OptionsServices.ReadOptions<OpenTelemetryOptions>(configuration, OtelSectionName);

    string otelExporterUrl = configuration["OpenTelemetry:OtelUrl"] ??
                             throw new InvalidOperationException("OTEL Exporter URL not informed");

    services.Configure<OtlpExporterOptions>(options => { options.Endpoint = new Uri(otelExporterUrl); });

    services.AddOpenTelemetry()
      .ConfigureResource(resource => resource
        .AddService(
          serviceName: openTelemetryOptions.ServiceName,
          serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString())
        .AddAttributes(new[]
        {
          new KeyValuePair<string, object>("deployment.environment", "Development"),
          new KeyValuePair<string, object>("team", "Platform"),
          new KeyValuePair<string, object>("system", "CloudMart"),
        }))
      .WithTracing(traces =>
      {
        traces.AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation()
          .AddMassTransitInstrumentation()
          .AddSource("Microsoft.Extensions.Resilience")
          .AddSource($"{openTelemetryOptions.ApplicationName}.*")
          .AddNpgsql()
          .AddOtlpExporter(options => { options.Endpoint = new Uri(otelExporterUrl); });
      })
      .WithMetrics(metrics =>
      {
        metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()
          .AddProcessInstrumentation()
          .AddOtlpExporter(options => { options.Endpoint = new Uri(otelExporterUrl); });
      });

    return services;
  }
}
