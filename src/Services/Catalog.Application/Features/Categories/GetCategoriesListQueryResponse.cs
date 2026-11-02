namespace Catalog.Application.Features.Categories;

public sealed record CategoryResponse
{
  public Guid Id { get; init; }

  public string Title { get; init; } = string.Empty;
};
