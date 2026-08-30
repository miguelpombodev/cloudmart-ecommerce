using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class DatabasesServices
{
  /// <summary>
  /// This method centralizes PostgreSQL databases configurations, so we don't need to worry about this configuration
  /// as dedicated service or method call
  /// </summary>
  /// <param name="services"></param>
  /// <param name="configuration"></param>
  /// <param name="environment"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static IServiceCollection AddDatabaseConfigurations<TContext>(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment environment)
  where TContext: DbContext
  {
    bool isDevelopment = environment.IsDevelopment();
    string connectionString =
      configuration.GetConnectionString("Database") ??
      throw new InvalidOperationException("No Connection String informed");

    var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
    {
      SslMode = isDevelopment ? SslMode.Disable : SslMode.Require, // TLS required in PRD
      Pooling = true,
      MinPoolSize = 1,
      MaxPoolSize = 20,
      Timeout = 15,
      CommandTimeout = 30,
      KeepAlive = 30, // Keep TCP connection alive through load balancers
      ApplicationName = environment.ApplicationName
    };

    services.AddDbContext<TContext>((serviceProvider, options) =>
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
}
