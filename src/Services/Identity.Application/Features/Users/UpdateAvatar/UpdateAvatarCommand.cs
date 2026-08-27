using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Identity.Application.Features.Users.UpdateAvatar;

public record UpdateAvatarCommand(Guid UserId, IFormFile File) : ICommand<Result<UpdateAvatarResponse>>
{
}
