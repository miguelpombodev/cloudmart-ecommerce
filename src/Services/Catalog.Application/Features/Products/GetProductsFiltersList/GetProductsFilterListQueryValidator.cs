using FluentValidation;

namespace Catalog.Application.Features.Products.GetProductsFiltersList;

public sealed class GetProductsFilterListQueryValidator : AbstractValidator<GetProductsFilterListRequest>
{
  public GetProductsFilterListQueryValidator()
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
  }
}
