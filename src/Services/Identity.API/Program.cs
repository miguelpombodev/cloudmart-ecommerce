using Cloudmart.Identity;
using Identity.Application;
using Identity.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Logging.AddLoggingBuilder(configuration);

builder.Host.AddHostBuilder();

builder.Services
  .AddInfrastructureServices(configuration)
  .AddTelemetryServices(configuration)
  .AddRepositories()
  .AddProviderOptions(configuration)
  .AddProviders(configuration)
  .AddApplicationServices()
  .AddApiServices();

WebApplication app = builder.Build();

app.UseApiServices();

await app.RunAsync();

/// <summary>
///   asasasaasas.
/// </summary>
// This is only for tests
public abstract partial class Program
{
}
