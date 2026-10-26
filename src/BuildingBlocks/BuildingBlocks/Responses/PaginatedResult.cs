using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Responses;

public sealed record PaginatedResult<T>(
  IReadOnlyList<T> Items,
  int TotalCount,
  bool HasNextPage,
  string? NextCursor = null
) : IPaginatedResult<T> where T : class;
