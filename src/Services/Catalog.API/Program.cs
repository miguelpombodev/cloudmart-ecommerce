using Catalog.Application;
using Catalog.Infrastructure;
using Cloudmart.Catalog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

builder.Logging.AddLoggingBuilder(configuration);

builder.Services
	.AddApplicationServices()
	.AddInfrastructureServices(configuration)
	.AddTelemetryServices(configuration)
	.AddApiServices();

WebApplication app = builder.Build();

app.UseApiServices();

await app.RunAsync();
