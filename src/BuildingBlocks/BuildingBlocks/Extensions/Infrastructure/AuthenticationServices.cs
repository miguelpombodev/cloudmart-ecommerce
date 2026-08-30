using System.Text;
using System.Text.Json;
using Identity.Application.Abstractions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Extensions.Infrastructure;

public static class AuthenticationServices
{
  public static IServiceCollection AddJwtAuthenticationWithCookie(this IServiceCollection services, IConfiguration configuration)
  {
     services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        JwtOptions jwtOptions =
          configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;

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
          OnMessageReceived = context =>
          {
            context.Token =
              context.Request.Cookies["access_token"];

            return Task.CompletedTask;
          },
          OnChallenge = context =>
          {
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            string body = JsonSerializer.Serialize(new
            {
              type = "https://tools.ietf.org/html/rfc7235#section-3.1",
              title = "Unauthorized",
              status = 401,
              detail = "A valid authentication cookie is required"
            });

            return context.Response.WriteAsync(body);
          },
          OnForbidden = context =>
          {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            string body = JsonSerializer.Serialize(new
            {
              type = "https://tools.ietf.org/html/rfc7235#section-3.1",
              title = "Forbidden",
              status = 403,
              detail = "You do not have permission to access this resource"
            });

            return context.Response.WriteAsync(body);
          }
        };
      });
    return services;
  }
}
