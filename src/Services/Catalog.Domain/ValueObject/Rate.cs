using BuildingBlocks.Exceptions;

namespace Catalog.Domain.ValueObject;

public class Rate : BuildingBlocks.Abstractions.ValueObject
{
  public decimal Value { get; private set; }

  private Rate()
  {
  }

  private Rate(decimal value)
  {
    Value = value;
  }

  public void SetValue(decimal value)
  {
    if (CheckIfRateValueIsHigherThanAccepted(value))
    {
      throw new DomainException("Review Rate cannot be higher or equal that 5.1");
    }

    Value = value;
  }


  public static Rate Create(decimal value)
  {
    if (CheckIfRateValueIsHigherThanAccepted(value))
    {
      throw new DomainException("Review Rate cannot be higher or equal that 5.1");
    }

    return new(value);
  }

  protected override IEnumerable<object?> RetrieveEqualityComponents()
  {
    yield return Value;
  }

  private static bool CheckIfRateValueIsHigherThanAccepted(decimal value) =>
    value >= (decimal)5.1;
}
