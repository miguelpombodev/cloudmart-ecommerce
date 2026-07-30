using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.RefreshTokens.Rotation;

public sealed class
  RefreshTokenRotationCommandHandler : ICommandHandler<RefreshTokenRotationCommand,
  Result<RefreshTokenRotationResponse>>
{
  private readonly ILogger<RefreshTokenRotationCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly ITokenService _tokenService;

  private readonly IUnitOfWork _uow;

  public RefreshTokenRotationCommandHandler(
    IUserRepository repository,
    ITokenService tokenService,
    IUnitOfWork uow,
    ILogger<RefreshTokenRotationCommandHandler> logger)
  {
    _repository = repository;
    _tokenService = tokenService;
    _uow = uow;
    _logger = logger;
  }

  public async Task<Result<RefreshTokenRotationResponse>> Handle(
    RefreshTokenRotationCommand request,
    CancellationToken cancellationToken)
  {
    string hashedOldRefreshToken = _tokenService.HashRefreshToken(request.OldRefreshToken);

    RefreshToken? getMostOldUserRefreshToken =
      await _repository.RetrieveLastOldRefreshToken(hashedOldRefreshToken);

    if (getMostOldUserRefreshToken is null)
    {
      return Result<RefreshTokenRotationResponse>.Failure(Error.Unauthorized("Refresh Token does not exist"));
    }

    if (hashedOldRefreshToken != getMostOldUserRefreshToken.Token ||
        getMostOldUserRefreshToken is
        {
          IsActive: false,
          IsExpired: true
        })
    {
      return Result<RefreshTokenRotationResponse>.Failure(Error.Unauthorized("Refresh Token must be valid"));
    }

    getMostOldUserRefreshToken.RevokeToken();

    User? user = await _repository.FindByIdAsync(getMostOldUserRefreshToken.UserId, cancellationToken);

    if (user is null)
    {
      return Result<RefreshTokenRotationResponse>.Failure(Error.Unauthorized("User's Refresh Token must be valid"));
    }

    TokenResult tokenResult = _tokenService.GenerateAccessToken(user);
    string rawRefreshToken = _tokenService.GenerateRefreshToken();
    string hashedRefreshToken = _tokenService.HashRefreshToken(rawRefreshToken);

    var refreshToken = RefreshToken.Create(user.Id, hashedRefreshToken, tokenResult.ExpiresAt);

    await _repository.AddRefreshToken(refreshToken, cancellationToken);
    _repository.UpdateRefreshToken(getMostOldUserRefreshToken);
    await _uow.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Refresh token rotated successfully");

    return Result<RefreshTokenRotationResponse>.Success(
      new RefreshTokenRotationResponse(
        tokenResult.AccessToken,
        rawRefreshToken));
  }
}
