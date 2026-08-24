using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Logging;
using Cloudmart.Identity.Configurations;
using Identity.Application;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Options;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Enums;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.Infrastructure.Providers;
using Identity.Infrastructure.Providers.Identity;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Grafana.Loki;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
  private const string ExternalProvidersSectionName = "Authentication";

  private const string OtelSectionName = "OpenTelemetry";

  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    string connectionString =
      configuration.GetConnectionString("Database") ??
      throw new InvalidOperationException("No Connection String informed");

    bool isDevelopment = configuration["ASPNETCORE_ENVIRONMENT"] == "Development";

    var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
    {
      SslMode = isDevelopment ? SslMode.Disable : SslMode.Require, // TLS required in PRD
      Pooling = true,
      MinPoolSize = 1,
      MaxPoolSize = 20,
      Timeout = 15,
      CommandTimeout = 30,
      KeepAlive = 30, // Keep TCP connection alive through load balancers
      ApplicationName = "Catalog.Api"
    };

    services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    {
      options.UseNpgsql(connectionStringBuilder.ConnectionString, pgAction =>
      {
        pgAction.EnableRetryOnFailure(
          3,
          TimeSpan.FromSeconds(5),
          null);

        pgAction.CommandTimeout(30);
        pgAction.MigrationsHistoryTable("__ef_migrations_history");
      });

      options.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
      options.EnableDetailedErrors(isDevelopment);
      options.EnableSensitiveDataLogging(isDevelopment);
    });


    return services;
  }

  public static ConfigureHostBuilder AddHostBuilder(this ConfigureHostBuilder host)
  {
    host.UseSerilog((context, services, loggerConfig) =>
    {
      loggerConfig
        .ReadFrom
        .Configuration(context.Configuration)
        .Enrich
        .FromLogContext()
        .Enrich
        .WithMachineName()
        .Enrich
        .WithEnvironmentName()
        .Enrich
        .WithThreadId()
        .Enrich
        .WithProperty("ServiceName", "Identity.Api")
        .Enrich
        .With<TraceIdEnricher>()
        .WriteTo
        .Console(new RenderedCompactJsonFormatter())
        .WriteTo
        .GrafanaLoki(
          context.Configuration["Loki:Url"]!,
          new[] { new LokiLabel { Key = "service", Value = "identity-api" } });
    });

    return host;
  }

  public static IServiceCollection AddLoggingServices(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment environment)
  {
    services.AddSerilog((svcs, loggerConfiguration) =>
    {
      loggerConfiguration
        .ReadFrom
        .Configuration(configuration)
        .ReadFrom
        .Services(svcs)
        .Enrich
        .WithProperty(
          "service.name",
          environment.ApplicationName)
        .Enrich
        .WithProperty(
          "deployment.environment",
          environment.EnvironmentName)
        .Enrich
        .WithEnvironmentName()
        .Enrich
        .WithMachineName()
        .Enrich
        .WithThreadId()
        .Enrich
        .WithOpenTelemetryTraceId()
        .Enrich
        .WithOpenTelemetrySpanId()
        .WriteTo
        .Console(outputTemplate:
          "[{Timestamp:HH:mm:ss} {Level:u3}] [{MachineName}] {Message:lj}{NewLine}{Exception}");
    });
    return services;
  }

  public static IServiceCollection AddTelemetryServices(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    OpenTelemetryOptions openTelemetryOptions = ReadOptions<OpenTelemetryOptions>(configuration, OtelSectionName);

    string otelExporterUrl = configuration["OpenTelemetry:OtelUrl"] ??
                             throw new InvalidOperationException("OTEL Exporter URL not informed");

    services.Configure<OtlpExporterOptions>(options => { options.Endpoint = new Uri(otelExporterUrl); });

    services.AddOpenTelemetry()
      .ConfigureResource(resource => resource
        .AddService(
          serviceName: openTelemetryOptions.ServiceName,
          serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString())
        .AddAttributes(new[]
        {
          new KeyValuePair<string, object>("deployment.environment", "Development"),
          new KeyValuePair<string, object>("team", "Platform"),
          new KeyValuePair<string, object>("system", "CloudMart"),
        }))
      .WithTracing(traces =>
      {
        traces.AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation()
          .AddMassTransitInstrumentation()
          .AddSource($"{openTelemetryOptions.ApplicationName}.*")
          .AddNpgsql()
          .AddOtlpExporter(options => { options.Endpoint = new Uri(otelExporterUrl); });
      })
      .WithMetrics(metrics =>
      {
        metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()
          .AddProcessInstrumentation()
          .AddOtlpExporter(options => { options.Endpoint = new Uri(otelExporterUrl); });
      });

    return services;
  }

  public static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
  }

  public static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services, IConfiguration configuration)
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

  public static IServiceCollection AddProviders(this IServiceCollection services, IConfiguration configuration)
  {
    ExternalProvidersOptions providersOptions =
      ReadOptions<ExternalProvidersOptions>(configuration, ExternalProvidersSectionName);

    services.AddScoped<ITokenService, TokenService>();

    services.AddHttpClient<IExternalIdentityProvider, GoogleAuthService>(client =>
    {
      client.BaseAddress = new Uri(providersOptions.Google.Url);
      client.Timeout = providersOptions.Google.ResponseTimeout;
    });

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        JwtOptions jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtOptions.Issuer,
          ValidAudience = jwtOptions.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
          ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
          OnChallenge = context =>
          {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            string body = JsonSerializer.Serialize(new
            {
              type = "https://tools.ietf.org/html/rfc7235#section-3.1",
              title = "Unauthorized",
              status = 401,
              detail =
                "A valid Bearer token is required"
            });

            return context.Response.WriteAsync(body);
          },
          OnForbidden = context =>
          {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            string body = JsonSerializer.Serialize(new
            {
              type = "https://tools.ietf.org/html/rfc7235#section-3.1",
              title = "Forbidden",
              status = 403,
              detail =
                "You do not have permission to access this resource"
            });

            return context.Response.WriteAsync(body);
          }
        };
      });

    bool isDevelopment = configuration["ASPNETCORE_ENVIRONMENT"] == "Development";

    services.AddAuthorization(options =>
    {
      options.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

      options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(nameof(RoleType.Admin)));
      options.AddPolicy(Policies.CatalogManagement, policy => policy.RequireRole("Admin", "Manager", "Seller"));

      options.AddPolicy(
        Policies.VerifiedSeller,
        policy => policy.RequireRole(nameof(RoleType.Seller)).RequireClaim("email_verified", "true"));

      if (!isDevelopment)
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
    services.Configure<OpenTelemetryOptions>(configuration.GetSection(OtelSectionName));

    return services;
  }

  private static T ReadOptions<T>(IConfiguration configuration, string sectionName)
  {
    return configuration.GetSection(sectionName).Get<T>() ??
           throw new InvalidOperationException(
             $"Configuration section '{ExternalProvidersSectionName}' is missing. " +
             $"Ensure appsettings contains the '{ExternalProvidersSectionName}' section.");
  }
}
