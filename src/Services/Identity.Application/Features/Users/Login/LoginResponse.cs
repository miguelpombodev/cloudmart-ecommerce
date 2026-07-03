namespace Identity.Application.Features.Users.Login;

public sealed record LoginResponse(string AccessToken, string RefreshToken);
