namespace Catalog.Application.Features.Brands.GetBrandsList;

public sealed record BrandDto
{
  public string Name { get; init; } = string.Empty;
  public string Url { get; init; } = string.Empty;
};

public sealed record GetBrandsListResponse(List<BrandDto> Brands);
