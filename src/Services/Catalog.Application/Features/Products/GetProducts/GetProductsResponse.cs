using Catalog.Domain.ValueObject;

namespace Catalog.Application.Features.Products.GetProducts;

public record GetProductsResponse(
  string name,
  decimal price,
  string currency,
  Image? image,
  string categoryTitle,
  Guid categoryId,
  string brandTitle,
  Guid brandId,
  string sku,
  string description,
  List<string> tags
);
