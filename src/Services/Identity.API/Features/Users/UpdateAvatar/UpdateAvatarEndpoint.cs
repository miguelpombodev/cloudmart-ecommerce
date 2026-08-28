using BuildingBlocks.Abstractions;
using Carter;
using Identity.Application.Features.Users.UpdateAvatar;
using Mapster;
using MediatR;

namespace Cloudmart.Identity.Features.Users.UpdateAvatar;

/// <inheritdoc />
public sealed class UpdateAvatarEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPatch("/user/avatar", async (
        IFormFile file,
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

        UpdateAvatarCommand command = new(userId, file);

        Result<UpdateAvatarResponse> result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        UpdateAvatarResponse response = result.Value.Adapt<UpdateAvatarResponse>();

        return Results.Ok(response);
      })
      .Accepts<IFormFile>("multipart/form-data")
      .RequireAuthorization()
      .WithName("UserAvatarUpdate")
      .Produces<UpdateAvatarResponse>()
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status500InternalServerError)
      .WithSummary("Create or update new user avatar image")
      .WithDescription("Receives a file, the service verifies its the extension and size, and at the end" +
                       "check if the user already has a avatar uploaded, if yes the image data will be updated" +
                       "otherwise it will be created");
  }
}
