namespace BuildingBlocks.Abstractions;

public interface IPaginatedResult<out T> where T : class
{
  IReadOnlyList<T> Items { get; }

  int TotalCount { get; }

  bool HasNextPage { get; }

  string? NextCursor { get; }
}
