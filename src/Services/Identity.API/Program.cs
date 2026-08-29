using Cloudmart.Identity;
using Identity.Application;
using Identity.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Host.AddHostBuilder();

builder.Services
  .AddProviderOptions(configuration)
  .AddInfrastructureServices(configuration)
  .AddMassTransitConfiguration(configuration)
  .AddTelemetryServices(configuration)
  .AddRepositories()
  .AddProviders(configuration, builder.Environment)
  .AddLoggingServices(configuration, builder.Environment)
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
