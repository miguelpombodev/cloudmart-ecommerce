using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using MediatR;

namespace Identity.Application.Features.Users.UpdateUser;

public record UpdateUserCommand(Guid UserId, string Email, string FirstName, string LastName): ICommand<Result<Unit>>;
