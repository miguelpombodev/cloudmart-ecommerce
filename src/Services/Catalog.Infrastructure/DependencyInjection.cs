using BuildingBlocks.Authorization;
using BuildingBlocks.Extensions.Infrastructure;
using BuildingBlocks.Infrastructure.Providers.Storage;
using BuildingBlocks.Options;
using Catalog.Infrastructure.Providers.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
  private const string StorageSectionName = "StorageProvider";

  public static IServiceCollection AddRepositories(this IServiceCollection services)
  {


    return services;
  }

  public static IServiceCollection AddAuthenticationWithPolicies(
    this IServiceCollection services,
    IWebHostEnvironment environment)
  {
    bool isProduction = environment.IsProduction();

    services.AddAuthorization(options =>
    {
      options.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

      options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(nameof(RoleType.Admin)));
      options.AddPolicy(Policies.CatalogManagement, policy => policy.RequireRole("Admin", "Manager", "Seller"));

      options.AddPolicy(
        Policies.VerifiedSeller,
        policy => policy.RequireRole(nameof(RoleType.Seller)).RequireClaim("email_verified", "true"));

      if (isProduction)
      {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
          .RequireAuthenticatedUser()
          .Build();
      }
    });

    return services;
  }

  public static IServiceCollection AddProviders(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment environment)
  {

    services.AddScoped<IStorageProvider, StorageServiceProvider>();

    return services;
  }

  public static IServiceCollection AddProviderOptions(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
    services.Configure<OpenTelemetryOptions>(configuration.GetSection(TelemetryServices.OtelSectionName));
    services.Configure<StorageProvider>(configuration.GetSection(StorageSectionName));

    return services;
  }
}
