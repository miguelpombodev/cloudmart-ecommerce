using BuildingBlocks.Abstractions;
using BuildingBlocks.Responses;
using Carter;
using Catalog.Application.Features.Products.GetProducts;
using Mapster;
using MediatR;

namespace Cloudmart.Catalog.Features.Products.GetProductsList;

/// <inheritdoc />
public class GetProductsListEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
        "/products",
        async (
          [AsParameters] GetProductsParameters parameters,
          ISender sender,
          CancellationToken cancellationToken) =>
        {
          GetProductsRequest request = new(
            parameters.Brands?.ToList(),
            parameters.Categories?.ToList(),
            parameters.MinValue,
            parameters.MaxValue,
            parameters.SortBy,
            parameters.PageSize,
            parameters.PageNumber);

          GetProductsQuery query = new(request);

          Result<IPaginatedResult<GetProductsResponse>> result = await sender.Send(query, cancellationToken);

          if (result.IsFailure)
          {
            return Results.Problem(
              statusCode: result.Error.StatusCode,
              detail: result.Error.Description,
              title: result.Error.InternalCode);
          }

          PaginatedResult<GetProductsResponse> response = result.Value.Adapt<PaginatedResult<GetProductsResponse>>();

          return Results.Created("/product/", response);
        })
      .WithName("GetProductsList")
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .WithSummary("Get products")
      .WithDescription("Gets a paginated and filterable list of products");
  }
}
