using Azure;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Identity.Application.Abstractions.Options;
using Identity.Application.Abstractions.Providers;
using Identity.Domain.Dtos.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Providers.Storage;

public sealed class StorageServiceProvider : IStorageProvider
{
  private readonly ILogger<StorageServiceProvider> _logger;

  private readonly BlobServiceClient _client;

  private const string ContainerName = "uploaded-users-avatars";

  private const string ImageContentType = "image/png";

  public StorageServiceProvider(ILogger<StorageServiceProvider> logger, IOptions<StorageProvider> options)
  {
    StorageProvider storageProviderValues = options.Value;
    BlobClientOptions blobClientOptions = new BlobClientOptions { Retry = { MaxRetries = 3, } };

    _logger = logger;

    var credentials =
      new StorageSharedKeyCredential(storageProviderValues.AccountName, storageProviderValues.AccountKey);

    _client = new
    (
      new Uri(storageProviderValues.StorageUrl),
      credentials,
      blobClientOptions
    );
  }

  public async Task<StorageUploadResponseDto> UploadImage(IFormFile file, CancellationToken ct)
  {
    string newFileName = NormalizeImageFileName();

    BlobContainerClient blobContainerClient = await CreateContainerIfNotExists(ct);

    _logger.LogInformation(
      "Accessing Azure Blob Container [{BlobContainerName}]",
      blobContainerClient.Name.ToUpperInvariant());

    BlobClient client =  RetrieveBlobClient(newFileName, blobContainerClient);

    _logger.LogInformation(
      "Create Blob client for file renamed as {BlobName}, and start to uploaded it",
      newFileName);

    await using Stream data = file.OpenReadStream();
    await client.UploadAsync(data, ct);

    _logger.LogInformation(
      "Blob named as {BlobName} uploaded successfully",
      newFileName);

    return new StorageUploadResponseDto
    (
      client.Uri.AbsoluteUri,
      client.Name,
      ImageContentType
    );
  }

  public Task<string> UploadFile() => throw new NotImplementedException();

  public async Task<bool> DeleteImage(string fileName, CancellationToken ct)
  {
    BlobContainerClient container = await CreateContainerIfNotExists(ct);
    BlobClient client = container.GetBlobClient(fileName);

    await client.DeleteAsync(cancellationToken: ct);

    return true;
  }

  public Task<bool> DeleteFile() => throw new NotImplementedException();

  private async Task<BlobContainerClient> CreateContainerIfNotExists(CancellationToken ct)
  {
    BlobContainerClient container = _client.GetBlobContainerClient(ContainerName);
    Response<bool> exists = await container.ExistsAsync(ct);

    if (!exists)
    {
      await container.CreateAsync(PublicAccessType.BlobContainer, cancellationToken: ct);
    }

    return container;
  }

  private static BlobClient RetrieveBlobClient(
    string fileName,
    BlobContainerClient containerClient)
  {
    BlobClient blobClient = containerClient.GetBlobClient(fileName);

    return blobClient;
  }

  private static string NormalizeImageFileName() =>
    $"customer_xxx_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
}
