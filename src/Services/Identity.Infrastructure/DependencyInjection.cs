using BuildingBlocks.Extensions.Infrastructure;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Options;
using Cloudmart.Identity.Configurations;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Options;
using Identity.Application.Abstractions.Providers;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Enums;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.Infrastructure.Providers;
using Identity.Infrastructure.Providers.Identity;
using Identity.Infrastructure.Providers.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
  private const string ExternalProvidersSectionName = "Authentication";

  private const string StorageSectionName = "StorageProvider";


  public static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
  }

  public static IServiceCollection AddProviders(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment environment)
  {
    ExternalProvidersOptions providersOptions =
      OptionsServices.ReadOptions<ExternalProvidersOptions>(configuration, ExternalProvidersSectionName);

    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IStorageProvider, StorageServiceProvider>();

    services.AddHttpClient<IExternalIdentityProvider, GoogleAuthService>(client =>
    {
      client.BaseAddress = new Uri(providersOptions.Google.Url);
      client.Timeout = providersOptions.Google.ResponseTimeout;
    });

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

  public static IServiceCollection AddProviderOptions(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
    services.Configure<ExternalProvidersOptions>(configuration.GetSection(ExternalProvidersSectionName));
    services.Configure<OpenTelemetryOptions>(configuration.GetSection(TelemetryServices.OtelSectionName));
    services.Configure<StorageProvider>(configuration.GetSection(StorageSectionName));

    return services;
  }
}
