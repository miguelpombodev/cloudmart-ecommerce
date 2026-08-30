using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class SecurityServices
{
  public static IServiceCollection AddAntiForgeryService(this IServiceCollection services)
  {
    services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN"; });

    return services;
  }
}
