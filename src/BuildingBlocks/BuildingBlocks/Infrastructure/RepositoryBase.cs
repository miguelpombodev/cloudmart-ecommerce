using BuildingBlocks.Abstractions;
using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure;

public abstract class RepositoryBase<TAggregate, TId, TContext>
  : IRepository<TAggregate, TId>
  where TAggregate : Aggregate<TId>
  where TContext : DbContext
{
  protected readonly TContext Context;

  protected readonly DbSet<TAggregate> DbSet;

  protected RepositoryBase(TContext ctx)
  {
    Context = ctx;
    DbSet = ctx.Set<TAggregate>();
  }


  public virtual async Task<TAggregate?> FindByIdAsync(TId id, CancellationToken ct = default) =>
    await DbSet.FindAsync([id], ct);

  public virtual async Task<IReadOnlyList<TAggregate>> FindAllAsync(CancellationToken ct = default) =>
    await DbSet.AsNoTracking().ToListAsync(ct);

  public virtual async Task AddAsync(TAggregate aggregate, CancellationToken ct = default) =>
    await DbSet.AddAsync(aggregate, ct);

  public virtual void UpdateAsync(TAggregate aggregate, CancellationToken none) =>
    DbSet.Update(aggregate);

  public virtual void RemoveAsync(TAggregate aggregate) =>
    DbSet.Remove(aggregate);
}
