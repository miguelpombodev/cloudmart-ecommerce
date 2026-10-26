using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Responses;
using Catalog.Domain.Entities;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Features.Products.GetProducts;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, Result<IPaginatedResult<GetProductsResponse>>>
{
  private readonly ApplicationDbContext _context;

  public GetProductsQueryHandler(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Result<IPaginatedResult<GetProductsResponse>>> Handle(
    GetProductsQuery parameters,
    CancellationToken cancellationToken)
  {
    IQueryable<GetProductsQueryFieldList> query = BuildQuery(parameters.request);

    PaginatedResult<GetProductsResponse> response = await RetrieveProductsWithPagination(query, parameters.request);

    return Result<IPaginatedResult<GetProductsResponse>>.Success(response);
  }

  private IQueryable<GetProductsQueryFieldList> BuildQuery(GetProductsRequest request)
  {
    IQueryable<GetProductsQueryFieldList> query = BuildBaseQuery();

    IQueryable<GetProductsQueryFieldList> queryFieldLists = BuildFilters(query, request);

    IQueryable<GetProductsQueryFieldList> orderedQueryFieldLists = AppliedSortInQuery(queryFieldLists, request);

    IQueryable<GetProductsQueryFieldList> paginatedQuery =
      ApplyPaginationParameters(orderedQueryFieldLists, request);

    return paginatedQuery;
  }

  private static async Task<PaginatedResult<GetProductsResponse>> RetrieveProductsWithPagination(
    IQueryable<GetProductsQueryFieldList> query,
    GetProductsRequest request)

  {
    List<GetProductsResponse> items = await query.Select(p => new GetProductsResponse(
        p.Title,
        p.Price,
        p.Currency,
        p.Sku,
        p.Description,
        p.Tags
      )
    ).ToListAsync();

    bool hasNext = items.Count > request.PageSize;

    return new PaginatedResult<GetProductsResponse>(items, items.Count, hasNext);
  }

  private static IQueryable<GetProductsQueryFieldList> ApplyPaginationParameters(
    IQueryable<GetProductsQueryFieldList> query,
    GetProductsRequest request)
  {
    query = query
      .Skip(request.PageNumber)
      .Take(request.PageSize + 1);

    return query;
  }

  private static IQueryable<GetProductsQueryFieldList>
    AppliedSortInQuery(
      IQueryable<GetProductsQueryFieldList> query,
      GetProductsRequest request
    )
  {
    query = request.SortBy switch
    {
      SortBy.HigherPrice => query.OrderByDescending(p => p.Price),
      SortBy.LowerPrice => query.OrderBy(p => p.Price),
      _ => query.OrderBy(p => p.Title)
    };

    return query;
  }

  private static IQueryable<GetProductsQueryFieldList> BuildFilters(
    IQueryable<GetProductsQueryFieldList> query,
    GetProductsRequest request)
  {
    if (request.Brands is { Count: > 0 })
    {
      query = query.Where(field => request.Brands.Contains(field.BrandTitle));
    }

    if (request.Categories is { Count: > 0 })
    {
      query = query.Where(field => request.Categories.Contains(field.CategoryTitle));
    }

    if (request.MinValue.HasValue)
    {
      query = query.Where(p => p.Price >= request.MinValue);
    }

    if (request.MaxValue.HasValue)
    {
      query = query.Where(p => p.Price <= request.MaxValue);
    }

    return query;
  }

  private IQueryable<GetProductsQueryFieldList> BuildBaseQuery()
  {
    IQueryable<GetProductsQueryFieldList> query = _context.Products.AsNoTracking()
      .Join(
        _context.Set<Brand>(),
        product => product.BrandId,
        brand => brand.Id,
        (product, brand) => new { product, brand })
      .Join(
        _context.Set<Category>(),
        join => join.product.CategoryId,
        category => category.Id,
        (join, category) => new GetProductsQueryFieldList
          {
          Title = join.product.Title.Value,
          Price = join.product.Price.Amount,
          Currency = join.product.Price.Currency,
          Sku = join.product.Sku,
          Description = join.product.Description,
          Tags = join.product.Tags,
          IsActive = join.product.IsActive,
          BrandId = join.brand.Id,
          BrandTitle = join.brand.Title.Value,
          CategoryTitle = category.Title.Value
        })
      .Where(product => product.IsActive);

    return query;
  }
}
