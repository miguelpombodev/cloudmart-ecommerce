using BuildingBlocks.Abstractions;
using BuildingBlocks.Responses;
using Carter;
using Catalog.Application.Features.Brands.GetBrandsList;
using Catalog.Application.Features.Products.GetProducts;
using Mapster;
using MediatR;

namespace Cloudmart.Catalog.Features.Brands.GetBrandsList;

/// <inheritdoc />
public sealed class GetBrandsListEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/brands", async (ISender sender, CancellationToken cancellationToken) =>
      {
        GetBrandsListQuery query = new();

        Result<GetBrandsListResponse> result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        GetBrandsListResponse response = result.Value.Adapt<GetBrandsListResponse>();

        return Results.Ok(response);
      })
      .WithName("GetBrandsList")
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .WithSummary("Get brands")
      .WithDescription("Gets a list of brands");
  }
}
