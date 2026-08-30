using BuildingBlocks.Logging;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Grafana.Loki;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class LoggingServices
{
  public static ConfigureHostBuilder AddSerilogConfigurations(
    this ConfigureHostBuilder host)
  {
    host.UseSerilog((context, services, loggerConfig) =>
    {
      string serviceName =
        context.Configuration["OpenTelemetry:ApplicationName"] ??
        throw new InvalidOperationException("OpenTelemetry Application name must be set, please review documentation");

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
        .WithProperty("ServiceName", serviceName)
        .Enrich
        .With<TraceIdEnricher>()
        .WriteTo
        .Console(new RenderedCompactJsonFormatter())
        .WriteTo
        .GrafanaLoki(
          context.Configuration["Loki:Url"]!,
          new[] { new LokiLabel { Key = "service", Value = serviceName} });
    });

    return host;
  }
}
