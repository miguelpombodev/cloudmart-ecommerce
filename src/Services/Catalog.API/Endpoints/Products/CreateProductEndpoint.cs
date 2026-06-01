using Carter;
using Catalog.Application.Products.CreateProduct;
using Mapster;
using MediatR;

namespace Cloudmart.Catalog.Endpoints.Products;

public record CreateProductRequest(
    string Name,
    List<string> Category,
    string Description,
    decimal Price,
    string ImageFile,
    int StockAmount);

public record CreateProductResponse(Guid Id);

public class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateProductCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreateProductResponse>();

                return Results.Created($"/product/{response.Id}", response);
            })
            .WithName("CreateProduct")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("CreateProduct")
            .WithDescription("CreateProduct");
    }
}