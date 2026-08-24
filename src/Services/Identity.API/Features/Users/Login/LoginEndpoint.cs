using BuildingBlocks.Abstractions;
using Carter;
using Identity.Application.Features.Users.Login;
using Identity.Application.Features.Users.Register;
using Mapster;
using MediatR;

namespace Cloudmart.Identity.Features.Users.Login;

/// <inheritdoc />
public sealed class LoginEndpoint : ICarterModule
{
  /// <summary>
  /// Login endpoints list.
  /// </summary>
  /// <param name="app">Group for endpoints URL.</param>
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost("/login", async (
        LoginRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken ct) =>
      {
        string clientIp = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                          httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        LoginCommand command = request.Adapt<LoginCommand>() with { ClientIp = clientIp };

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
      .WithName("Login")
      .AllowAnonymous()
      .Produces<LoginResponse>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .WithSummary("Authenticate user and issue tokens")
      .AllowAnonymous();
  }
}
