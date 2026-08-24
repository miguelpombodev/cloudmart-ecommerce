using BuildingBlocks.Abstractions;
using Carter;
using Identity.Application.Features.Users.Login;
using Identity.Application.Features.Users.SocialLogin;
using Mapster;
using MediatR;

namespace Cloudmart.Identity.Features.Users.Login;

/// <inheritdoc />
public sealed class GoogleLoginEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost("/auth/google", async (
        GoogleLoginRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken ct) =>
      {
        string clientIp = httpContext.Request.Headers["X-Forwarded-For"]
                            .FirstOrDefault() ??
                          httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new GoogleLoginCommand(
          request.idToken,
          clientIp);

        Result<LoginResponse> result = await sender.Send(command, ct);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        LoginResponse response = result.Value.Adapt<LoginResponse>();

        httpContext.Response.Cookies.Append(
          "access_token",
          response.AccessToken,
          new CookieOptions
          {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = response.ExpiresAt,
          });

        return Results.Ok(new { response.RefreshToken });
      })
      .WithName("GoogleLogin")
      .AllowAnonymous()
      .Produces<LoginResponse>()
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .WithSummary("Authenticate user via Google OAuth2")
      .WithDescription("Receives a Google id_token obtained by the frontend " +
                       "and issues a CloudMart JWT. Use Google OAuth Playground " +
                       "to obtain a test id_token.");
  }
}
