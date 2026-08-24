using BuildingBlocks.Abstractions;
using BuildingBlocks.Middlewares;
using Carter;
using Cloudmart.Identity.Services;
using Scalar.AspNetCore;

namespace Cloudmart.Identity;

internal static class DependencyInjection
{
  public static IServiceCollection AddApiServices(this IServiceCollection services)
  {
    services.AddRouting();
    services.AddOpenApi();
    services.AddCarter();

    services.AddTransient<GlobalExceptionHandlerMiddleware>();

    services.AddHttpContextAccessor();
    services.AddScoped<ICurrentUser, CurrentUser>();

    return services;
  }

  public static WebApplication UseApiServices(this WebApplication app)
  {
    app.UseRouting();
    app.MapGroup("/api/v1/identity").MapCarter();

    if (app.Environment.IsDevelopment())
    {
      app.MapOpenApi();
      app.MapScalarApiReference(ConfigureScalar).AllowAnonymous();
    }

    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<LogEnrichmentMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();

    return app;
  }

  private static void ConfigureScalar(ScalarOptions x)
  {
    x.Title = "Identity.API";
    x.Theme = ScalarTheme.BluePlanet;
    x.DarkMode = true;
  }
}
