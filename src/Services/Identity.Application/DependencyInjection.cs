using System.Reflection;
using BuildingBlocks.Behaviors;
using FluentValidation;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    var applicationAssembly = Assembly.GetExecutingAssembly();

    services.AddMediatR(x =>
    {
      x.RegisterServicesFromAssemblies(applicationAssembly);
      x.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    services.AddValidatorsFromAssembly(applicationAssembly);

    TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
    config.Scan(Assembly.GetExecutingAssembly());

    return services;
  }
}
