using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
    config.Scan(Assembly.GetExecutingAssembly());

    services.AddMediatR(x => { x.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()); });

    return services;
  }
}
