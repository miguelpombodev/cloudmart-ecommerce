using BuildingBlocks.Extensions.API;
using BuildingBlocks.Extensions.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

builder.Host.AddSerilogConfigurations();

builder.Services
  .AddProviderOptions(configuration)
  .AddTelemetryProviderConfiguration(configuration)
  .AddDatabaseConfigurations<ApplicationDbContext>(configuration, environment)
  .AddRepositories()
  .AddMassTransitConfiguration(configuration)
  .AddProviders(configuration, builder.Environment)
  .AddJwtAuthenticationWithCookie(configuration)
  .AddAuthenticationWithPolicies(builder.Environment)
  .AddNonHttpResilience()
  .AddStorageProviderResilience()
  .AddAntiForgeryService()
  .AddCQRSRegistration()
  .AddFluentValidationConfiguration()
  .AddServicesHealthChecks(configuration, environment)
  .AddApiServices();

WebApplication app = builder.Build();

app
  .MapServicesHealthChecks(environment)
  .UseApiServices();

await app.RunAsync();

/// <summary>
///   asasasaasas.
/// </summary>
// This is only for tests
public abstract partial class Program
{
}
