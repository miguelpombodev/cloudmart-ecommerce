using Identity.Domain.ValueObject;

namespace Identity.Application.Features.Users.GetInformations;

public sealed record GetInformationsResponse(
  string Name,
  string Email,
  bool HasAvatar,
  string? AvatarUrl,
  string Initials,
  DateTimeOffset? CreatedAt
  );
