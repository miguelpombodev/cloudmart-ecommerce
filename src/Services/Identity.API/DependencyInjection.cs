using BuildingBlocks.Abstractions;
using BuildingBlocks.Extensions;
using BuildingBlocks.Extensions.API;
using BuildingBlocks.Middlewares;
using Carter;
using Cloudmart.Identity.Services;

namespace Cloudmart.Identity;

internal static class DependencyInjection
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

    app.MapGroup("/api/v1/identity").MapCarter();

    return app;
  }
}
