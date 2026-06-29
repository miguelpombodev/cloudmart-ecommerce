namespace Identity.Infrastructure.Exceptions;

public sealed class DbException : Exception
{
  public DbException(string message) : base(message)
  {
  }
}
