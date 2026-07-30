using BuildingBlocks.Abstractions;
using Carter;
using Identity.Application.Features.RefreshTokens.Rotation;
using Identity.Application.Features.Users.Login;
using Mapster;
using MediatR;

namespace Cloudmart.Identity.Features.RefreshToken.Rotation;

/// <inheritdoc />
public class RefreshTokenRotationEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    RouteGroupBuilder group = app.MapGroup("refresh-token");

    group.MapPost("rotation", async (
        RefreshTokenRotationRequest request,
        ISender sender,
        CancellationToken ct) =>
      {
        RefreshTokenRotationCommand command = request.Adapt<RefreshTokenRotationCommand>();

        Result<RefreshTokenRotationResponse> result = await sender.Send(command, ct);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        LoginResponse response = result.Value.Adapt<LoginResponse>();

        return Results.Ok(response);
      })
      .WithSummary("Rotate Refresh Token")
      .WithName("Rotate Refresh Token")
      .WithDescription("Endpoint with objective to rotate user's old refresh token")
      .Produces<RefreshTokenRotationResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest);
  }
}
