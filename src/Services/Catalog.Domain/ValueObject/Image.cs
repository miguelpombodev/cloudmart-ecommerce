using BuildingBlocks.Exceptions;

namespace Catalog.Domain.ValueObject;

public sealed class Image : BuildingBlocks.Abstractions.ValueObject
{
  public string Url { get; }
  public string AltText { get; }
  public bool IsPrimary { get; }

  private Image()
  {
    Url = null!;
    AltText = null!;
  }

  private Image(string url, string altText, bool isPrimary)
  {
    Url = url;
    AltText = altText;
    IsPrimary = isPrimary;
  }

  public static Image Create(string url, string altText, bool isPrimary = false)
  {
    if (string.IsNullOrWhiteSpace(url))
    {
      throw new DomainException("Image URL cannot be empty");
    }

    return new(url, altText, isPrimary);
  }

  protected override IEnumerable<object?> RetrieveEqualityComponents()
  {
    yield return Url;
    yield return IsPrimary;
  }
}
