using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Identity.Application.Features.RefreshTokens.Rotation;

public record RefreshTokenRotationCommand(string AccessToken, string OldRefreshToken)
  : ICommand<Result<RefreshTokenRotationResponse>>;
