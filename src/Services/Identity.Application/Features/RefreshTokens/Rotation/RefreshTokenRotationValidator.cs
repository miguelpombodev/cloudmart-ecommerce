using FluentValidation;

namespace Identity.Application.Features.RefreshTokens.Rotation;

public sealed class RefreshTokenRotationValidator : AbstractValidator<RefreshTokenRotationCommand>
{
  public RefreshTokenRotationValidator()
  {
    RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Access token is required");

    RuleFor(x => x.OldRefreshToken)
      .NotEmpty()
      .WithMessage("Expired refresh token is required")
      .MinimumLength(10)
      .WithMessage("Invalid refresh token format");
  }
}
