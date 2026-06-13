namespace Catalog.Domain.ValueObject;

public sealed class Money : BuildingBlocks.Abstractions.ValueObject
{
  private Money()
  {
    Currency = null!;
  }

  private Money(decimal amount, string currency)
  {
    Amount = amount;
    Currency = currency;
  }

  public decimal Amount { get; }
  public string Currency { get; }

  public static Money Create(decimal amount, string currency)
  {
    if (amount < 0)
    {
      throw new DomainException("Amount cannot be negative");
    }

    if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
    {
      throw new DomainException("Currency must be a 3-letter ISO code.");
    }

    return new Money(amount, currency.ToUpperInvariant());
  }

  public static Money Zero(string currency) => new(0, currency);

  public Money Add(Money other)
  {
    if (Currency != other.Currency)
    {
      throw new DomainException("Cannot add money with different currencies");
    }

    return new Money(Amount + other.Amount, Currency);
  }

  public override string ToString() => $"{Amount:F2} {Currency}";

  protected override IEnumerable<object?> RetrieveEqualityComponents()
  {
    yield return Amount;
    yield return Currency;
  }
}
