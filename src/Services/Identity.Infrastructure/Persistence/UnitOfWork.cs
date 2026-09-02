using BuildingBlocks.Infrastructure;
using Identity.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Identity.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
  private readonly ApplicationDbContext _context;

  private readonly ILogger<UnitOfWork> _logger;

  public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
  {
    _context = context;
    _logger = logger;
  }

  public Task<int> SaveChangesAsync(CancellationToken ct = default)
  {
    try
    {
      return _context.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException e)
    {
      _logger.LogCritical(
        e,
        "[CRITICAL] A concurrency violation is encountered when trying to saving any data in context - Message: {ErrorMessage} Trace: {ErrorTrace}",
        e.Message,
        e.StackTrace);

      throw new DbException(" A concurrency violation is encountered when trying to saving data");
    }
    catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
    {
      _logger.LogCritical("Duplicate entry detected during retry for {Entries}", ex.Entries);
      throw new DbException("An error occurred while persisting data");
    }

    catch (DbUpdateException ex)
    {
      _logger.LogCritical(
        ex,
        "[CRITICAL] There was an error when trying to saving any data in context - Message: {ErrorMessage} Trace: {ErrorTrace}",
        ex.Message,
        ex.StackTrace);
      throw new DbException("An error occurred while persisting data");
    }
  }
}
