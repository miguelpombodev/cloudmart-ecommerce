using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Identity.Application.Features.Users.Login;

public sealed class LoginCommandHandler: ICommandHandler<LoginCommand, Result<LoginResponse>>
{
  public Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
