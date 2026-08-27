namespace Identity.Application.Abstractions.Options;

public sealed class StorageProvider
{
  public string AccountName { get; set; } = string.Empty;
  public string AccountKey { get; set; } = string.Empty;
  public string StorageUrl { get; set; } = string.Empty;

  public string StorageProviderName { get; set; } = string.Empty;
}
