using Identity.Domain.ValueObject;

namespace Identity.Application.Features.Users.GetInformations;

public sealed record GetInformationsResponse(
  string Name,
  DateTimeOffset? CreatedAt
  );
