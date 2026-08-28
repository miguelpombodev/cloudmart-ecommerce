using System.Data;
using FluentValidation;

namespace Identity.Application.Features.Users.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
  public UpdateUserCommandValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .WithMessage("Email address is required")
      .EmailAddress()
      .WithMessage("Email must be a valid address");

    RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
    RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
  }
}
