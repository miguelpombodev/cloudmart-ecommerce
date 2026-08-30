using BuildingBlocks.Extensions.API;
using BuildingBlocks.Extensions.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Host.AddSerilogConfigurations();


builder.Services
  .AddTelemetryProviderConfiguration(configuration)
  .AddAntiForgeryService()
  .AddReverseProxy()
  .LoadFromConfig(configuration.GetSection("ReverseProxy"));

WebApplication app = builder.Build();

app.UseEndpointsServiceResources();

app.MapReverseProxy();

await app.RunAsync();
