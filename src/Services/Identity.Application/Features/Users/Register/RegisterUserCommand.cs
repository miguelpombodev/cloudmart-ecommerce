using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Identity.Application.Features.Users.Register;

public sealed record RegisterUserCommand(
  string FirstName,
  string LastName,
  string Email,
  string Password
) : ICommand<Result<RegisterUserResponse>>;
