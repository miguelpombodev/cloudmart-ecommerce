using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Providers.Storage;

public interface IStorageProvider
{
  Task<StorageUploadResponseDto> UploadImage(IFormFile file, CancellationToken ct);
  Task<string> UploadFile();
  Task<bool> DeleteImage(string fileName, CancellationToken ct);
  Task<bool> DeleteFile();
}
