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
  private static readonly Assembly ApplicationAssembly = Assembly.GetExecutingAssembly();

  public static IServiceCollection AddCQRSRegistration(this IServiceCollection services)
  {
    services.AddMediatR(x =>
    {
      x.RegisterServicesFromAssemblies(ApplicationAssembly);
      x.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    return services;
  }

  public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
  {
    services.AddValidatorsFromAssembly(ApplicationAssembly);

    TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
    config.Scan(Assembly.GetExecutingAssembly());

    return services;
  }

}
