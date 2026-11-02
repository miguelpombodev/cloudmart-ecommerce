namespace Catalog.Application.Features.Products.GetProductsFiltersList;

public sealed class FilterProp
{
  public string Id { get; set; } = string.Empty;

  public string Title { get; set; } = string.Empty;

  public int Quantity { get; set; }
}

public sealed class FacetItem
{
  public string Facet { get; set; }= string.Empty;
  public string Id { get; set; }= string.Empty;
  public string Title { get; set; }= string.Empty;
  public int Quantity { get; set; }
};

public sealed class GetProductsFilterListResponse
{
  public GetProductsFilterListResponse(
    IList<FilterProp> brandsItems,
    IList<FilterProp> categoriesItems,
    IList<FilterProp> reviewsItems)
  {
    Brands = brandsItems;
    Categories = categoriesItems;
    Reviews = reviewsItems;
  }

  public IList<FilterProp> Brands { get; }

  public IList<FilterProp> Categories { get; }

  public IList<FilterProp> Reviews { get; }
};
