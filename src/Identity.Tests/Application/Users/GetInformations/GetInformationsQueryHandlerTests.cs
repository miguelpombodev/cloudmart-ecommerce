using BuildingBlocks.Abstractions;
using FluentAssertions;
using Identity.Application.Abstractions.Repositories;
using Identity.Application.Features.Users.GetInformations;
using Identity.Domain.Entities;
using Identity.Tests.Domain.Builder;
using Moq;
using Polly;
using Polly.Registry;

namespace Identity.Tests.Application.Users.GetInformations;

public class GetInformationsQueryHandlerTests
{
  private readonly GetInformationsQueryHandler _handler;

  private readonly Mock<IUserRepository> _repositoryMock;

  private readonly Mock<ResiliencePipelineProvider<string>> _pipeline;

  public GetInformationsQueryHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _pipeline = new Mock<ResiliencePipelineProvider<string>>();

    _handler = new GetInformationsQueryHandler(
      _repositoryMock.Object,
      _pipeline.Object
    );
  }

  [Fact]
  public async Task Handle_WhenUserDoesNotExist_ShouldReturnConflict()
  {
    // Arrange
    Guid userId = Guid.NewGuid();

    var query = new GetInformationsQuery(userId);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        userId,
        CancellationToken.None))
      .ReturnsAsync((User?)null);

    // Act
    Result<GetInformationsResponse> result =
      await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.StatusCode.Should().Be(409);
    result.Error.Description.Should().Be("User not registered!");

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        userId,
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenUserExistsWithoutAvatar_ShouldReturnUserInformations()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    var query = new GetInformationsQuery(user.Id);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None))
      .ReturnsAsync(user);

    // Act
    Result<GetInformationsResponse> result =
      await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    result.Value.Should().NotBeNull();

    result.Value.Name.Should().Be(user.Name.ToString());
    result.Value.Email.Should().Be(user.Email.Address);

    result.Value.HasAvatar.Should().BeFalse();
    result.Value.AvatarUrl.Should().BeNull();

    result.Value.Initials.Should().Be(user.Name.Initials);
    result.Value.CreatedAt.Should().Be(user.CreatedAt);

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenUserExistsWithAvatar_ShouldReturnUserInformationsWithAvatar()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    user.SetAvatar(
      "https://storage.test/avatars/john.jpg",
      "john.jpg",
      "image/jpeg");

    var query = new GetInformationsQuery(user.Id);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None))
      .ReturnsAsync(user);

    // Act
    Result<GetInformationsResponse> result =
      await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    result.Value.Should().NotBeNull();

    result.Value.Name.Should().Be(user.Name.ToString());
    result.Value.Email.Should().Be(user.Email.Address);

    result.Value.HasAvatar.Should().BeTrue();

    result.Value.AvatarUrl.Should().Be(
      "https://storage.test/avatars/john.jpg");

    result.Value.Initials.Should().Be(user.Name.Initials);
    result.Value.CreatedAt.Should().Be(user.CreatedAt);

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldNotCallRepositoryMoreThanOnce()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    var query = new GetInformationsQuery(user.Id);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None))
      .ReturnsAsync(user);

    // Act
    Result<GetInformationsResponse> result =
      await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None),
      Times.Once);
  }
}
