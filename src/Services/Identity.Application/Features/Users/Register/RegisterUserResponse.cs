namespace Identity.Application.Features.Users.Register;

public sealed record RegisterUserResponse(Guid Id, string FullName, string Email, DateTimeOffset CreatedAt);
