using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using MediatR;

namespace Identity.Application.Features.Users.InactivateUser;

public record InactivateUserCommand(Guid UserId) : ICommand<Result<Unit>>;
