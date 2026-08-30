using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class OptionsServices
{
  public static T ReadOptions<T>(IConfiguration configuration, string sectionName)
  {
    return configuration.GetSection(sectionName).Get<T>() ??
           throw new InvalidOperationException(
             $"Configuration section '{sectionName}' is missing. " +
             $"Ensure appsettings contains the '{sectionName}' section.");
  }
}
