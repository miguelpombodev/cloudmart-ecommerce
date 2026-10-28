using Carter;

namespace Cloudmart.Catalog.Features.Products.GetProductsFiltersList;

/// <inheritdoc />
public sealed class GetProductsFiltersListEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/products/filters", () => new { message = "done!" });
  }
}
