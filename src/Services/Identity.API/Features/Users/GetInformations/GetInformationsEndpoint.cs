using BuildingBlocks.Abstractions;
using Carter;
using Identity.Application.Features.Users.GetInformations;
using Identity.Application.Features.Users.Register;
using Mapster;
using MediatR;

namespace Cloudmart.Identity.Features.Users.GetInformations;

/// <inheritdoc />
public sealed class GetInformationsEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/user/me", async (ICurrentUser currentUser, ISender sender, CancellationToken cancellationToken) =>
      {
        Guid userId = currentUser.UserId;

        if (
          string.IsNullOrWhiteSpace(userId.ToString()) &&
          currentUser.IsAuthenticated)
        {
          return Results.Unauthorized();
        }

        GetInformationsQuery query = new (userId);

        Result<GetInformationsResponse> result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        GetInformationsResponse response = result.Value.Adapt<GetInformationsResponse>();

        return Results.Ok(response);
      })
      .RequireAuthorization()
      .WithName("GetCurrentUser")
      .Produces<GetInformationsResponse>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .WithSummary("Get authenticated user information");
  }
}
