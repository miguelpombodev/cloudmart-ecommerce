using Cloudmart.Identity;
using Cloudmart.Identity.Middlewares;
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
  .AddApplicationServices()
  .AddApiServices();

WebApplication app = builder.Build();

app.UseApiServices();

app.UseMiddleware<LogEnrichmentMiddleware>();

await app.RunAsync();
