using Carter;
using MediatR;

namespace Cloudmart.Catalog.Endpoints.Products;

/// <inheritdoc />
public class CreateProductEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
        "/products",
        async (ISender sender) => { return Results.Created("/product/", new { teste = "teste" }); })
      .WithName("CreateProduct")
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .WithSummary("CreateProduct")
      .WithDescription("CreateProduct");
  }
}
