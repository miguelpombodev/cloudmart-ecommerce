WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

builder.Logging.AddLoggingBuilder(configuration);

builder.Services
  .AddApplicationServices()
  .AddInfrastructureServices(configuration)
  .AddTelemetryServices(configuration)
  .AddApiServices();

builder.Services.AddMediatR(x => { x.RegisterServicesFromAssemblies(typeof(Program).Assembly); });

WebApplication app = builder.Build();

app.UseApiServices();

await app.RunAsync();
