namespace Catalog.Domain.ValueObject;

public class Review : BuildingBlocks.Abstractions.ValueObject
{
  public string AuthorName { get; set; }
  public string? Description { get; set; }
  public float Rate { get; set; }

  private Review()
  {
    AuthorName = null!;
  }

  private Review(string authorName, string description, float rate)
  {
    AuthorName = authorName;
    Description = description;
    Rate = rate;
  }

  public static Review Create(string authorName, string description, float rate = 0)
  {
    if (string.IsNullOrWhiteSpace(authorName))
      throw new DomainException("Author Name cannot be empty");

    if (rate >= 5.1)
      throw new DomainException("Review Rate cannot be higher or equal that 5.1");

    return new(authorName, description, rate);
  }


  protected override IEnumerable<object?> RetrieveEqualityComponents()
  {
    yield return AuthorName;
    yield return Description;
    yield return Rate;
  }
}
