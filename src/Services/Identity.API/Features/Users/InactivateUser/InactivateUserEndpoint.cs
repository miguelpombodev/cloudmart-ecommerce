using BuildingBlocks.Abstractions;
using Carter;
using Identity.Application.Features.Users.InactivateUser;
using MediatR;

namespace Cloudmart.Identity.Features.Users.InactivateUser;

/// <inheritdoc />
public sealed class InactivateUserEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete("/user", async (
        ICurrentUser currentUser,
        ISender sender,
        CancellationToken cancellationToken) =>
      {
        Guid userId = currentUser.UserId;

        if (
          string.IsNullOrWhiteSpace(userId.ToString()) &&
          currentUser.IsAuthenticated)
        {
          return Results.Unauthorized();
        }

        InactivateUserCommand command = new(userId);

        Result<Unit> result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        return Results.Ok();
      })
      .RequireAuthorization()
      .WithName("InactivateUser")
      .Produces(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .WithSummary("Inactivate User");
  }
}
