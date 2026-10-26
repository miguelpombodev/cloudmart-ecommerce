using Catalog.Domain.ValueObject;

namespace Catalog.Application.Features.Products.GetProducts;

public record GetProductsResponse(
  string name,
  decimal price,
  string currency,
  string sku,
  string description,
  List<string> tags
  );
