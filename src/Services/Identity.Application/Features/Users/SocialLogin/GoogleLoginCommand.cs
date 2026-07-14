using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using Identity.Application.Features.Users.Login;

namespace Identity.Application.Features.Users.SocialLogin;

public record GoogleLoginCommand(string IdToken, string ClientIp = "") : ICommand<Result<LoginResponse>>;
