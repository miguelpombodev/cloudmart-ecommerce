using Carter;

namespace Cloudmart.Catalog.Features.Products.GetProductsFiltersList;

public sealed class GetProductsFiltersListEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/products/filters", () => new { message = "done!" });
  }
}
