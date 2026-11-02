using Microsoft.AspNetCore.Mvc;

namespace Catalog.Application.Features.Products.GetProductsFiltersList;

public sealed class GetProductsFilterListRequest
{
  [FromQuery(Name = "brands")] public Guid[]? Brands { get; set; } = [];

  [FromQuery(Name = "categories")] public Guid[]? Categories { get; set; } = [];

  [FromQuery(Name = "minValue")] public decimal? MinValue { get; set; } = null;

  [FromQuery(Name = "maxValue")] public decimal? MaxValue { get; set; } = null;
}
