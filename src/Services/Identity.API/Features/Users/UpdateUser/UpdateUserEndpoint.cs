using BuildingBlocks.Abstractions;
using Carter;
using Identity.Application.Features.Users.UpdateUser;
using MediatR;

namespace Cloudmart.Identity.Features.Users.UpdateUser;

/// <inheritdoc />
public sealed class UpdateUserEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut("/user",
        async (
          ICurrentUser currentUser,
          UpdateUserRequest request,
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

          UpdateUserCommand command = new(userId, request.Email, request.FirstName, request.LastName);

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
      .WithSummary("Update User Data")
      .WithName("UpdateUserData")
      .WithDescription("Endpoint with objective to update user's data")
      .Produces(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status400BadRequest);
  }
}
