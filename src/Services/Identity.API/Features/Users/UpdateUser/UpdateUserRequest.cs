namespace Cloudmart.Identity.Features.Users.UpdateUser;

/// <summary>
///
/// </summary>
/// <param name="Email"></param>
/// <param name="FirstName"></param>
/// <param name="LastName"></param>
public sealed record UpdateUserRequest(string Email, string FirstName, string LastName);
