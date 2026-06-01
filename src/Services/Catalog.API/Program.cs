using Catalog.Application;
using Catalog.Infrastructure;
using Cloudmart.Catalog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices().AddInfrastructureServices(builder.Configuration).AddApiServices();

WebApplication app = builder.Build();

app.UseApiServices();

await app.RunAsync();
