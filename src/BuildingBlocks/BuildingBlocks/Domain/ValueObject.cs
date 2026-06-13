namespace BuildingBlocks.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
  public bool Equals(ValueObject? other)
  {
    if (other is null)
    {
      return false;
    }

    return RetrieveEqualityComponents().SequenceEqual(other.RetrieveEqualityComponents());
  }

  protected abstract IEnumerable<object?> RetrieveEqualityComponents();

  public override bool Equals(object? obj)
  {
    if (obj is null)
    {
      return false;
    }

    if (ReferenceEquals(this, obj))
    {
      return true;
    }

    if (obj.GetType() != GetType())
    {
      return false;
    }

    return Equals((ValueObject)obj);
  }

  public override int GetHashCode() =>
    RetrieveEqualityComponents().Aggregate(
        new HashCode(),
        (hash, component) =>
        {
          hash.Add(component);

          return hash;
        })
      .ToHashCode();

  public static bool operator ==(ValueObject? left, ValueObject? right)
  {
    if (left is null && right is null)
    {
      return true;
    }

    if (left is null || right is null)
    {
      return false;
    }

    return left.Equals(right);
  }

  public static bool operator !=(ValueObject? left, ValueObject? right) =>
    !(left == right);
}
