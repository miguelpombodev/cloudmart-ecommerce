using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Features.Categories;

public sealed class
  GetCategoriesListQueryHandler : IQueryHandler<GetCategoriesListQuery, Result<List<CategoryResponse>>>
{
  private readonly ApplicationDbContext _context;

  public GetCategoriesListQueryHandler(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Result<List<CategoryResponse>>> Handle(
    GetCategoriesListQuery request,
    CancellationToken cancellationToken)
  {
    List<CategoryResponse> response = await _context.Categories.AsNoTracking()
      .Select(x =>
        new CategoryResponse { Id = x.Id, Title = x.Title.Value, ImageUrl = x.Image.Url})
      .ToListAsync(cancellationToken);


    return Result<List<CategoryResponse>>.Success(response);
  }
}
