using BuildingBlocks.Abstractions;
using Carter;
using Identity.Application.Features.Users.Register;
using Mapster;
using MediatR;

namespace Cloudmart.Identity.Features.Users.Create;

/// <inheritdoc />
public sealed class RegisterUserEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost("/register", async (RegisterUserRequest request, ISender sender) =>
      {
        RegisterUserCommand command = request.Adapt<RegisterUserCommand>();

        Result<RegisterUserResponse> result = await sender.Send(command);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        RegisterUserResponse response = result.Value.Adapt<RegisterUserResponse>();

        return Results.Created($"/{response.Id}", response);
      })
      .WithName("RegisterUser")
      .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .WithSummary("Register User")
      .WithDescription("Register User");
  }
}
