using BuildingBlocks.Abstractions;
using BuildingBlocks.Context;
using BuildingBlocks.Extensions.API;
using BuildingBlocks.Middlewares;
using Carter;
using Scalar.AspNetCore;

namespace Cloudmart.Catalog;

internal static class DependencyInjectionExtensions
{


  public static IServiceCollection AddApiServices(this IServiceCollection services)
  {
    services.AddEndpointsServiceResources();

    services.AddTransient<GlobalExceptionHandlerMiddleware>();
    services.AddScoped<ICurrentUser, CurrentUser>();

    return services;
  }

  public static WebApplication UseApiServices(this WebApplication app)
  {
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<LogEnrichmentMiddleware>();

    app.UseEndpointsServiceResources();

    app.MapGroup("/api/v1/catalog").MapCarter();

    return app;
  }
}
