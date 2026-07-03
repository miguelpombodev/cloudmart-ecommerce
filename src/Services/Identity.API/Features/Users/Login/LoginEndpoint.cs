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
    app.MapPost("/login", async (LoginRequest request, ISender sender) =>
    {
      LoginCommand command = request.Adapt<LoginCommand>();

      Result<LoginResponse> result = await sender.Send(command);

      if (result.IsFailure)
      {
        return Results.Problem(
          statusCode: result.Error.StatusCode,
          detail: result.Error.Description,
          title: result.Error.InternalCode);
      }

      LoginResponse response = result.Value.Adapt<LoginResponse>();

      return Results.Ok(response);
    }).WithDescription("Login endpoint");
  }
}
