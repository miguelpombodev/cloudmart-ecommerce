using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions;
using Identity.Application.Abstractions.Auth;
using Identity.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Users.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
  private readonly ILogger<LoginCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly ITokenService _tokenService;

  private readonly IUnitOfWork _uow;

  public LoginCommandHandler(
    IUserRepository repository,
    ITokenService tokenService,
    IUnitOfWork uow,
    ILogger<LoginCommandHandler> logger)
  {
    _repository = repository;
    _tokenService = tokenService;
    _uow = uow;
    _logger = logger;
  }

  public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    User? user = await _repository.FindByEmail(request.Email);

    if (user is null)
    {
      return Result<LoginResponse>.Failure(Error.Unauthorized("Invalid Credentials"));
    }

    if (!user.Password.Verify(request.Password) || !user.IsActive)
    {
      return Result<LoginResponse>.Failure(Error.Unauthorized("Invalid Credentials"));
    }

    TokenResult tokenResult = _tokenService.GenerateAccessToken(user);
    string rawRefreshToken = _tokenService.GenerateRefreshToken();
    string hashedRefreshToken = _tokenService.HashRefreshToken(rawRefreshToken);

    var refreshToken = RefreshToken.Create(user.Id, hashedRefreshToken, tokenResult.ExpiresAt);

    await _repository.AddRefreshToken(refreshToken, cancellationToken);
    await _uow.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User {UserEmail} logged successfully", user.RetrieveMaskedEmail());

    return Result<LoginResponse>.Success(
      new LoginResponse(
        tokenResult.AccessToken,
        rawRefreshToken,
        900,
        tokenResult.TokenType));
  }
}
