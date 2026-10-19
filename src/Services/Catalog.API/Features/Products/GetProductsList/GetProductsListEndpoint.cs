using Carter;

namespace Cloudmart.Catalog.Features.Products.GetProductsList;

/// <inheritdoc />
public class GetProductsListEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/products", () =>
    {
      return new { message = "done!" };
    });
  }
}
