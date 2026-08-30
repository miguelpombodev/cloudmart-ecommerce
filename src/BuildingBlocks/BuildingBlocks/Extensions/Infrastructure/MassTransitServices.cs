using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class MassTransitServices
{
  public static IServiceCollection AddMassTransitConfiguration(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    services.AddMassTransit(busConfigurator =>
    {
      busConfigurator.SetKebabCaseEndpointNameFormatter();

      busConfigurator.UsingRabbitMq((context, config) =>
      {
        config.ConfigureJsonSerializerOptions(options =>
        {
          options.Converters.Add(new JsonStringEnumConverter());

          return options;
        });

        config.Host(new Uri(configuration["MessageBroker:Host"]!), h =>
        {
          h.Username(configuration["MessageBroker:Username"]!);
          h.Password(configuration["MessageBroker:Password"]!);
        });

        config.ConfigureEndpoints(context);
      });
    });

    return services;
  }
}
