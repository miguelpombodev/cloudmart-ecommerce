using Carter;
using Scalar.AspNetCore;

namespace Cloudmart.Identity;

internal static class DependencyInjection
{
  public static IServiceCollection AddApiServices(this IServiceCollection services)
  {
    services.AddRouting();
    services.AddOpenApi();
    services.AddCarter();

    return services;
  }

  public static WebApplication UseApiServices(this WebApplication app)
  {
    app.UseRouting();
    app.MapGroup("/api/v1/identity").MapCarter();

    if (app.Environment.IsDevelopment())
    {
      app.MapOpenApi();
      app.MapScalarApiReference(ConfigureScalar);
    }

    return app;
  }

  private static void ConfigureScalar(ScalarOptions x)
  {
    x.Title = "Identity.API";
    x.Theme = ScalarTheme.BluePlanet;
    x.DarkMode = true;
  }
}
