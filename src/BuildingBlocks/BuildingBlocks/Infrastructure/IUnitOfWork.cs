namespace BuildingBlocks.Infrastructure;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken ct = default);
}
