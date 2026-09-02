using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class ResilienceExtensions
{
  public static IServiceCollection AddNonHttpResilience(this IServiceCollection services)
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

  public static IServiceCollection AddStorageProviderResilience(
    this IServiceCollection services)
  {
    services.AddResiliencePipeline("storage-operation", builder =>
    {
      builder
        .AddTimeout(TimeSpan.FromSeconds(30))
        .AddRetry(new RetryStrategyOptions
        {
          MaxRetryAttempts = 3,
          Delay = TimeSpan.FromSeconds(1),
          BackoffType = DelayBackoffType.Exponential,
          UseJitter = true,
          ShouldHandle = new PredicateBuilder()
            .Handle<TimeoutException>()
            .Handle<RequestFailedException>()
            .Handle<IOException>()
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
          SamplingDuration = TimeSpan.FromSeconds(30),
          FailureRatio = 0.5,
          MinimumThroughput = 3,
          BreakDuration = TimeSpan.FromSeconds(30),
          ShouldHandle = new PredicateBuilder()
            .Handle<TimeoutException>()
            .Handle<RequestFailedException>()
        });
    });

    return services;
  }
}
