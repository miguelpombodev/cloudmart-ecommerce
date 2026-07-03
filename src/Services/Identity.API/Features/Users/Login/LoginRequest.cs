namespace Cloudmart.Identity.Features.Users.Login;

/// <summary>
/// DTO for login credentials
/// </summary>
/// <param name="email">User's email</param>
/// <param name="password">User's password</param>
public sealed record LoginRequest(string email, string password);
