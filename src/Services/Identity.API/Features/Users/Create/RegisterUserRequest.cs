namespace Cloudmart.Identity.Features.Users.Create;

/// <summary>
/// </summary>
/// <param name="firstName"></param>
/// <param name="lastName"></param>
/// <param name="email"></param>
/// <param name="password"></param>
public record RegisterUserRequest(
  string firstName,
  string lastName,
  string email,
  string password);
