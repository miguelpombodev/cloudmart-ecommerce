using BuildingBlocks.Abstractions;
using Carter;
using Catalog.Application.Features.Categories;
using Mapster;
using MediatR;

namespace Cloudmart.Catalog.Features.Categories;

/// <inheritdoc />
public sealed class GetCategoriesListEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/categories", async (ISender sender, CancellationToken cancellationToken) =>
      {
        GetCategoriesListQuery query = new();

        Result<List<CategoryResponse>> result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
          return Results.Problem(
            statusCode: result.Error.StatusCode,
            detail: result.Error.Description,
            title: result.Error.InternalCode);
        }

        List<CategoryResponse> response = result.Value.Adapt<List<CategoryResponse>>();

        return Results.Ok(response);
      })
      .WithName("GetCategoriesList")
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .WithSummary("Get categories")
      .WithDescription("Gets a list of categories");
  }
}
