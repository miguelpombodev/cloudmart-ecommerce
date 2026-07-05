using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Identity.Application.Features.Users.Login;

public sealed record LoginCommand(
  string Email,
  string Password,
  string ClientIp = "") : ICommand<Result<LoginResponse>>;
