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

    RefreshToken? oldToken =
      await _repository.RetrieveLastOldRefreshToken(hashedOldRefreshToken);

    if (oldToken is null)
    {
      _logger.LogWarning("Refresh token rotation attempt with unknown token");

      return Result<RefreshTokenRotationResponse>.Failure(Error.Unauthorized("Refresh Token does not exist"));
    }

    if (hashedOldRefreshToken != oldToken.Token)
    {
      return Result<RefreshTokenRotationResponse>.Failure(Error.Unauthorized("Refresh Token must be valid"));
    }

    if (oldToken.IsRevoked)
    {
      _logger.LogCritical(
        "[INTEGRITY_ERROR] Refresh token reuse detected for user {UserId}. ", oldToken.UserId);
    }

    if (oldToken is
        {
          IsActive: false,
          IsExpired: true
        })
    {
      _logger.LogInformation(
        "[INTEGRITY_ERROR] Expired refresh token rotation attempt for user {UserId}",
        oldToken.UserId);
    }

    oldToken.RevokeToken();

    User? user = await _repository.FindByIdAsync(oldToken.UserId, cancellationToken);

    if (user is null)
    {
      _logger.LogError(
        "[INTEGRITY_ERROR] Refresh token {TokenId} exists but user {UserId} not found",
        oldToken.Id,
        oldToken.UserId);

      return Result<RefreshTokenRotationResponse>.Failure(Error.Unauthorized("User's Refresh Token must be valid"));
    }

    TokenResult tokenResult = _tokenService.GenerateAccessToken(user);
    string rawRefreshToken = _tokenService.GenerateRefreshToken();
    string hashedRefreshToken = _tokenService.HashRefreshToken(rawRefreshToken);

    var refreshToken = RefreshToken.Create(user.Id, hashedRefreshToken, tokenResult.ExpiresAt);

    await _repository.AddRefreshToken(refreshToken, cancellationToken);
    _repository.UpdateRefreshToken(oldToken);
    await _uow.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
      "Refresh token rotated for user {UserId}. " +
      "Old token {OldTokenId} revoked. New token issued.",
      user.Id,
      oldToken.Id);

    return Result<RefreshTokenRotationResponse>.Success(
      new RefreshTokenRotationResponse(
        tokenResult.AccessToken,
        rawRefreshToken));
  }
}
