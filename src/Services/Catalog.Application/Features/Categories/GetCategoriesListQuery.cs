using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Catalog.Application.Features.Categories;

public sealed record GetCategoriesListQuery : IQuery<Result<List<CategoryResponse>>>;
