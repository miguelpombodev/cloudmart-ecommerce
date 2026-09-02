using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Repositories;
using Identity.Application.Features.Users.Login;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.ValueObject;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Identity.Application.Features.Users.SocialLogin;

public sealed class GoogleLoginCommandHandler : ICommandHandler<GoogleLoginCommand, Result<LoginResponse>>
{
  private readonly IExternalIdentityProvider _googleProvider;

  private readonly ILogger<GoogleLoginCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly IRoleRepository _roleRepository;

  private readonly ResiliencePipeline _databasePipeline;

  private readonly ITokenService _tokenService;

  private readonly IUnitOfWork _uow;

  public GoogleLoginCommandHandler(
    IUserRepository repository,
    IRoleRepository roleRepository,
    ITokenService tokenService,
    IExternalIdentityProvider googleProvider,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<GoogleLoginCommandHandler> logger,
    IUnitOfWork uow)
  {
    _repository = repository;
    _roleRepository = roleRepository;
    _tokenService = tokenService;
    _googleProvider = googleProvider;
    _logger = logger;
    _uow = uow;
    _databasePipeline = pipelineProvider.GetPipeline("database-operations");
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

    User? user =
      await _databasePipeline.ExecuteAsync(async ct => await _repository.FindByEmail(externalUser.Email, ct));

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
      await _databasePipeline.ExecuteAsync(async ct => await _repository.FindUserAuthenticationByProviderAsync(
          externalUser.Email,
          externalUser.Provider,
          ct
        )
      );

    if (checkUserAuthenticationProvider is null)
    {
      var userAuthenticationProvider =
        UserAuthenticationProvider.Create(
          user.Id,
          externalUser.Provider,
          externalUser.ProviderId,
          user.Email
        );

      await _databasePipeline.ExecuteAsync(async ct =>
        await _repository.AddUserAuthenticationProviderAsync(
          userAuthenticationProvider,
          ct
        )
      );
    }

    TokenResult tokenResult = _tokenService.GenerateAccessToken(user);
    string rawRefreshToken = _tokenService.GenerateRefreshToken();
    string hashedToken = _tokenService.HashRefreshToken(rawRefreshToken);

    var refreshToken = RefreshToken.Create(
      user.Id, hashedToken, tokenResult.ExpiresAt);

    await _databasePipeline.ExecuteAsync(async ct =>
    {
      await _repository.AddRefreshToken(refreshToken, ct);
      await _uow.SaveChangesAsync(ct);
    });

    _logger.LogInformation(
      "User {Email} logged in via {Provider}",
      externalUser.Email, externalUser.Provider);

    return Result<LoginResponse>.Success(new LoginResponse(
      tokenResult.AccessToken,
      rawRefreshToken,
      DateTimeOffset.UtcNow.AddMinutes(2),
      tokenResult.TokenType));
  }

  private async Task<User> CreateUserAuthenticated(ExternalUserInfo externalUser, CancellationToken ct)
  {
    Role customerRole = await _databasePipeline.ExecuteAsync(async ct =>
      await _roleRepository.FindRoleByName(
        nameof(RoleType.Customer),
        ct
      )
    );

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
