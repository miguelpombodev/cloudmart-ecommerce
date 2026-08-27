using Identity.Domain.Dtos.Storage;
using Microsoft.AspNetCore.Http;

namespace Identity.Application.Abstractions.Providers;

public interface IStorageProvider
{
  Task<StorageUploadResponseDto> UploadImage(IFormFile file, CancellationToken ct);
  Task<string> UploadFile();
  Task<bool> DeleteImage(string fileName, CancellationToken ct);
  Task<bool> DeleteFile();
}
