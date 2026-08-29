using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace BuildingBlocks.Extensions.API;

public static class ScalarConfig
{
  /// <summary>
  /// Implements Scalar configuration in application to access endpoints by Scalars page
  /// </summary>
  /// <param name="app">microservice's <c>WebApplication</c> instance</param>
  /// <returns><c>WebApplication</c> instance</returns>
  public static WebApplication AddScalarConfiguration(this WebApplication app)
  {
    if (app.Environment.IsDevelopment())
    {
      app.MapOpenApi();
      app.MapScalarApiReference(ConfigureScalar).AllowAnonymous();
    }

    return app;
  }

  /// <summary>
  /// Add the pipelines for routing, OpenApi, Carter library and also HttpContextAccessor to reach requests context
  /// </summary>
  /// <param name="services">microservice's <c>IServiceCollection</c> instance</param>
  /// <returns><c>IServiceCollection</c> instance</returns>
  public static IServiceCollection AddEndpointsServiceResources(this IServiceCollection services)
  {
    services.AddRouting();
    services.AddOpenApi();
    services.AddCarter();
    services.AddHttpContextAccessor();

    return services;
  }

  /// <summary>
  /// Apply the pipelines for routing, OpenApi, Carter library and also HttpContextAccessor to reach requests context
  ///
  /// <remarks>WARNING: Configuration method <c>AddEndpointsServiceResources</c> must be called before this methods</remarks>
  /// </summary>
  /// <param name="app">microservice's <c>WebApplication</c> instance</param>
  /// <returns><c>WebApplication</c> instance</returns>
  public static WebApplication UseEndpointsServiceResources(this WebApplication app)
  {
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.AddScalarConfiguration();

    app.UseAntiforgery();

    return app;
  }

  private static void ConfigureScalar(ScalarOptions x)
  {
    x.Title = "Identity.API";
    x.Theme = ScalarTheme.BluePlanet;
    x.DarkMode = true;
  }
}
