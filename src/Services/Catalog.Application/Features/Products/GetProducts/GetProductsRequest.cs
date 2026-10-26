using System.ComponentModel;
using System.Text.Json.Serialization;
using BuildingBlocks.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Application.Features.Products.GetProducts;

public enum SortBy
{
  HigherPrice,
  LowerPrice,
}

public record GetProductsRequest(
  List<string>? Brands,
  List<string>? Categories,
  decimal? MinValue,
  decimal? MaxValue,
  SortBy? SortBy = null,
  int PageSize = 20,
  int PageNumber = 1)
  : PageParameters(PageSize, PageNumber);


public sealed class GetProductsParameters
{
  [FromQuery(Name = "brands")]
  public string[]? Brands { get; set; }

  [FromQuery(Name = "categories")]
  public string[]? Categories { get; set; }

  [FromQuery(Name = "minValue")]
  public decimal? MinValue { get; set; }

  [FromQuery(Name = "maxValue")]
  public decimal? MaxValue { get; set; }

  [FromQuery(Name = "sortBy")]
  public SortBy? SortBy { get; set; }

  [FromQuery(Name = "pageSize")]
  [DefaultValue(20)]
  public int PageSize { get; set; } = 20;

  [FromQuery(Name = "pageNumber")]
  [DefaultValue(1)]
  public int PageNumber { get; set; } = 1;
}
