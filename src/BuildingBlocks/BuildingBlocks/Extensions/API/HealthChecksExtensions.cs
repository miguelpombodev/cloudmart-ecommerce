using BuildingBlocks.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using RabbitMQ.Client;

namespace BuildingBlocks.Extensions.API;

public static class HealthChecksExtensions
{
  private const string LivenessUri = "/health/live";

  private const string DetailedUri = "/health/detailed";

  public static IServiceCollection AddServicesHealthChecks(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment environment)
  {
    string databaseConnectionString = configuration.GetConnectionString("Database")!;
    EventHubOptions rabbitOptions = configuration.GetSection("MessageBroker").Get<EventHubOptions>()!;

    services
      .AddHealthChecks()
      .AddCheck(
        "self",
        () => HealthCheckResult.Healthy($"{environment.ApplicationName} is running"),
        tags: ["live"]
      )
      .AddNpgSql(
        databaseConnectionString,
        name: "postgresql",
        timeout: TimeSpan.FromSeconds(5),
        tags: ["ready", "db"]
      )
      .AddRabbitMQ(_ =>
        {
          var connectionFactory = new ConnectionFactory
          {
            HostName = rabbitOptions.Host.Contains("localhost") ? "localhost" : "",
            UserName = rabbitOptions.Username,
            Password = rabbitOptions.Password,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
          };

          return connectionFactory.CreateConnectionAsync();
        },
        name: $"{environment.ApplicationName.ToLowerInvariant().Replace(" ", "_")}.{rabbitOptions.ProviderName}",
        timeout: TimeSpan.FromSeconds(3),
        tags: ["ready", "messaging"]
      )
      .ForwardToPrometheus();

    return services;
  }

  public static WebApplication MapServicesHealthChecks(this WebApplication app, IWebHostEnvironment environment)
  {
    app.MapHealthChecks(LivenessUri,
      new HealthCheckOptions { Predicate = check => check.Tags.Contains("live"), ResponseWriter = WriteResponseAsync });

    app.MapHealthChecks(DetailedUri,
      new HealthCheckOptions { Predicate = _ => true, ResponseWriter = WriteResponseAsync });

    app.UseHttpMetrics();
    app.MapMetrics("/metrics");

    return app;
  }

  private static Task WriteResponseAsync(HttpContext context, HealthReport report)
  {
    context.Response.ContentType = "application/json";

    var response = new
    {
      status = report.Status.ToString(),
      totalDurationMs = report.TotalDuration.TotalMilliseconds,
      checks = report.Entries.Select(entry => new
      {
        name = entry.Key,
        status = entry.Value.Status.ToString(),
        description = entry.Value.Description,
        durationMs = entry.Value.Duration.TotalMilliseconds,
        error = entry.Value.Exception?.Message,
        tags = entry.Value.Tags
      })
    };

    return context.Response.WriteAsJsonAsync(response);
  }
}
