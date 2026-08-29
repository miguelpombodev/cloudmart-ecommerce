using FluentValidation;
using FluentValidation.Validators;

namespace Identity.Application.Features.Users.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
  public LoginCommandValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty().
      WithMessage("Email is required").
      EmailAddress(EmailValidationMode.Net4xRegex)
      .WithMessage("Email address is not valid");

    RuleFor(x => x.Password).
      MinimumLength(8).
      WithMessage("Password must have at least 8 characters");
  }
}
