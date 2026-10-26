namespace Catalog.Application.Features.Products.GetProducts;

public record GetProductsQueryFieldList
{
  public string Title { get; init; } = string.Empty;

  public decimal Price { get; init; }

  public string Currency { get; init; } = string.Empty;

  public string Sku { get; init; } = string.Empty;

  public string Description { get; init; } = string.Empty;

  public List<string> Tags { get; init; } = [];

  public bool IsActive { get; init; }

  public Guid BrandId { get; init; }

  public string BrandTitle { get; init; } = string.Empty;

  public string CategoryTitle { get; init; } = string.Empty;
}
