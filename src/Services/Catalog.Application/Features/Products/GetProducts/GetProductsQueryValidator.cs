using System.Data;
using FluentValidation;

namespace Catalog.Application.Features.Products.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsParameters>
{
  public GetProductsQueryValidator()
  {
    RuleForEach(x => x.Brands)
      .NotEqual(Guid.Empty)
      .WithMessage("Brand ID must not be an empty UUID.");

    RuleForEach(x => x.Categories)
      .NotEqual(Guid.Empty)
      .WithMessage("Category ID must not be an empty UUID.");

    RuleFor(x => x.MinValue)
      .GreaterThanOrEqualTo(0)
      .When(x => x.MinValue.HasValue)
      .WithMessage("Minimum value must be greater than or equal to zero.");

    RuleFor(x => x.MaxValue)
      .GreaterThanOrEqualTo(0)
      .When(x => x.MaxValue.HasValue)
      .WithMessage("Maximum value must be greater than or equal to zero.");

    RuleFor(x => x)
      .Must(x =>
        !x.MinValue.HasValue ||
        !x.MaxValue.HasValue ||
        x.MinValue <= x.MaxValue)
      .WithMessage("Minimum value cannot be greater than maximum value.");

    RuleFor(x => x.SortBy)
      .IsInEnum()
      .When(x => x.SortBy.HasValue)
      .WithMessage("Invalid sort option.");

    RuleFor(x => x.PageSize)
      .InclusiveBetween(1, 100)
      .WithMessage("Page size must be between 1 and 100.");

    RuleFor(x => x.PageNumber)
      .GreaterThanOrEqualTo(1)
      .WithMessage("Page number must be greater than or equal to 1.");
  }
}
