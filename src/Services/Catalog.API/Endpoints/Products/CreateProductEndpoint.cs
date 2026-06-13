using Carter;
using Catalog.Application.Products.CreateProduct;
using Mapster;
using MediatR;

namespace Cloudmart.Catalog.Endpoints.Products;

public record CreateProductRequest(
  string name,
  List<string> category,
  string description,
  decimal price,
  string imageFile,
  int stockAmount);

public record CreateProductResponse(Guid id);

public class CreateProductEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost("/products", async (CreateProductRequest request, ISender sender) =>
      {
        CreateProductCommand command = request.Adapt<CreateProductCommand>();

        CreateProductResult result = await sender.Send(command);

        CreateProductResponse response = result.Adapt<CreateProductResponse>();

        return Results.Created($"/product/{response.id}", response);
      })
      .WithName("CreateProduct")
      .Produces<CreateProductResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .WithSummary("CreateProduct")
      .WithDescription("CreateProduct");
  }
}
