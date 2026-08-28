namespace BuildingBlocks.Abstractions;

public interface IRepository<TAggregate, TId> where TAggregate : IAggregate
{
  Task<TAggregate?> FindByIdAsync(TId id, CancellationToken ct = default);
  Task<IReadOnlyList<TAggregate>> FindAllAsync(CancellationToken ct = default);

  Task AddAsync(TAggregate aggregate, CancellationToken ct = default);
  void UpdateAsync(TAggregate aggregate, CancellationToken none);
  void RemoveAsync(TAggregate aggregate);
}
