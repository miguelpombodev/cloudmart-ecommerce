using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polly;
using Polly.Retry;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class ResilienceExtensions
{
  public static IServiceCollection AddNonHttpResilience(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddResiliencePipeline("database-operations", builder =>
    {
      builder
        .AddTimeout(TimeSpan.FromSeconds(30))
        .AddRetry(new RetryStrategyOptions
        {
          MaxRetryAttempts = 2,
          Delay = TimeSpan.FromMicroseconds(500),
          BackoffType = DelayBackoffType.Exponential,
          UseJitter = true,
          ShouldHandle = new PredicateBuilder()
            .Handle<TimeoutException>()
            .Handle<NpgsqlException>(ex => ex.IsTransient)
        });
    });

    return services;
  }

  public static IServiceCollection AddStorageProviderResilience(this IServiceCollection services, IConfiguration configuration)
  {

    return services;
  }
}
