using Cloudmart.Identity;
using Identity.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Logging.AddLoggingBuilder(configuration);

builder.Services
  .AddInfrastructureServices(configuration)
  .AddTelemetryServices(configuration)
  .AddApiServices();

builder.Services.AddMediatR(x => { x.RegisterServicesFromAssemblies(typeof(Program).Assembly); });

WebApplication app = builder.Build();

app.UseApiServices();

await app.RunAsync();
