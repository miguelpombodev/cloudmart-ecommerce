using BuildingBlocks.Extensions.Infrastructure;
using Cloudmart.Identity;
using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
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
  .AddNonHttpResilience(configuration)
  .AddAntiForgeryService()
  .AddCQRSRegistration()
  .AddFluentValidationConfiguration()
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
