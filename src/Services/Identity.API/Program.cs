using Identity.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Logging.AddLoggingBuilder(configuration);

builder.Services.AddOpenApi();

builder.Services
  .AddInfrastructureServices(configuration)
  .AddTelemetryServices(configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

await app.RunAsync();
