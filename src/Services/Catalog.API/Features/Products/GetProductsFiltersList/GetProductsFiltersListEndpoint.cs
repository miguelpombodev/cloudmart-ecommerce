using BuildingBlocks.Abstractions;
using Carter;
using Catalog.Application.Features.Products.GetProductsFiltersList;
using Mapster;
using MediatR;

namespace Cloudmart.Catalog.Features.Products.GetProductsFiltersList;

/// <inheritdoc />
public sealed class GetProductsFiltersListEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/products/filters", async (
        [AsParameters] GetProductsFilterListRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
      {
        GetProductsFiltersListQuery query = new(request);

        Result<GetProductsFilterListResponse> result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        GetProductsFilterListResponse response = result.Value.Adapt<GetProductsFilterListResponse>();

        return Results.Ok(response);
      })
      .WithName("GetProductsFiltersList")
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .WithSummary("Get products filters list")
      .WithDescription("Gets a list of filters for products list");
  }
}
