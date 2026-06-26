using FluentValidation;

namespace Identity.Application.Features.Users.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
  public RegisterUserCommandValidator()
  {
    RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
    RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");

    RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress()
      .WithMessage("Email address is not valid");

    RuleFor(x => x.Password).MinimumLength(8).WithMessage("Password must have at least 8 characters");
  }
}
