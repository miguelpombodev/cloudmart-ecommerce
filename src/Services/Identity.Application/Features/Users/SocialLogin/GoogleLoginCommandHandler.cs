using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Features.Users.Login;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.ValueObject;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Users.SocialLogin;

public sealed class GoogleLoginCommandHandler : ICommandHandler<GoogleLoginCommand, Result<LoginResponse>>
{
  private readonly IExternalIdentityProvider _googleProvider;

  private readonly ILogger<GoogleLoginCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly ITokenService _tokenService;

  private readonly IUnitOfWork _uow;

  public GoogleLoginCommandHandler(
    IUserRepository repository,
    ITokenService tokenService,
    IExternalIdentityProvider googleProvider,
    ILogger<GoogleLoginCommandHandler> logger,
    IUnitOfWork uow)
  {
    _repository = repository;
    _tokenService = tokenService;
    _googleProvider = googleProvider;
    _logger = logger;
    _uow = uow;
  }

  public async Task<Result<LoginResponse>> Handle(
    GoogleLoginCommand request,
    CancellationToken cancellationToken)
  {
    ExternalUserInfo? externalUser = await _googleProvider.ValidateTokenAsync(
      request.IdToken, cancellationToken);

    if (externalUser is null)
    {
      return Result<LoginResponse>.Failure(
        Error.Unauthorized("Invalid or expired Google token"));
    }

    if (string.IsNullOrWhiteSpace(externalUser.Email))
    {
      return Result<LoginResponse>.Failure(
        Error.Unauthorized("Google account does not have a verified email"));
    }

    User? user = await _repository.FindByEmail(externalUser.Email);

    if (user is null)
    {
      user = await CreateUserAuthenticated(externalUser, cancellationToken);
    }
    else if (!user.IsActive)
    {
      return Result<LoginResponse>.Failure(
        Error.Unauthorized("Invalid credentials"));
    }

    UserAuthenticationProvider? checkUserAuthenticationProvider =
      await _repository.FindUserAuthenticationByProviderAsync(
        externalUser.Email,
        externalUser.Provider,
        cancellationToken);

    if (checkUserAuthenticationProvider is null)
    {
      var userAuthenticationProvider =
        UserAuthenticationProvider.Create(
          user.Id,
          externalUser.Provider,
          externalUser.ProviderId,
          user.Email
        );

      await _repository.AddUserAuthenticationProviderAsync(userAuthenticationProvider, cancellationToken);
    }

    TokenResult tokenResult = _tokenService.GenerateAccessToken(user);
    string rawRefreshToken = _tokenService.GenerateRefreshToken();
    string hashedToken = _tokenService.HashRefreshToken(rawRefreshToken);

    var refreshToken = RefreshToken.Create(
      user.Id, hashedToken, tokenResult.ExpiresAt);

    await _repository.AddRefreshToken(refreshToken, cancellationToken);

    await _uow.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
      "User {Email} logged in via {Provider}",
      externalUser.Email, externalUser.Provider);

    return Result<LoginResponse>.Success(new LoginResponse(
      tokenResult.AccessToken,
      rawRefreshToken,
      900,
      tokenResult.TokenType));
  }

  private async Task<User> CreateUserAuthenticated(ExternalUserInfo externalUser, CancellationToken ct)
  {
    Role customerRole = await _repository.FindRoleByName(nameof(RoleType.Customer), ct);

    var completeName = CompleteName.Create(
      externalUser.FirstName ?? externalUser.Email.Split('@')[0],
      externalUser.LastName ?? "User");

    var email = Email.Create(externalUser.Email);
    var password = Password.CreateWithProvider(null);

    var user = User.Create(
      completeName,
      email,
      password,
      customerRole);

    await _repository.AddAsync(user, ct);

    _logger.LogInformation(
      "New user created via {Provider} login: {Email}",
      externalUser.Provider, externalUser.Email);

    return user;
  }
}
