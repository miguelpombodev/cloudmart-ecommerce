using BuildingBlocks.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.Registry;
using FormFile = Microsoft.AspNetCore.Http.FormFile;
using HeaderDictionary = Microsoft.AspNetCore.Http.HeaderDictionary;
using IFormFile = Microsoft.AspNetCore.Http.IFormFile;
using IStorageProvider = Identity.Application.Abstractions.Providers.IStorageProvider;
using It = Moq.It;
using IUnitOfWork = BuildingBlocks.Infrastructure.IUnitOfWork;
using IUserRepository = Identity.Application.Abstractions.Repositories.IUserRepository;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using StorageUploadResponseDto = Identity.Domain.Dtos.Storage.StorageUploadResponseDto;
using Times = Moq.Times;
using UpdateAvatarCommand = Identity.Application.Features.Users.UpdateAvatar.UpdateAvatarCommand;
using UpdateAvatarCommandHandler = Identity.Application.Features.Users.UpdateAvatar.UpdateAvatarCommandHandler;
using UpdateAvatarResponse = Identity.Application.Features.Users.UpdateAvatar.UpdateAvatarResponse;
using User = Identity.Domain.Entities.User;
using UserAvatar = Identity.Domain.Entities.UserAvatar;
using UserBuilder = Identity.Tests.Domain.Builder.UserBuilder;

namespace Identity.Tests.Application.Users.UpdateAvatar;

public class UpdateAvatarCommandHandlerTests
{
  private readonly UpdateAvatarCommandHandler _handler;

  private readonly Mock<IUserRepository> _repositoryMock;

  private readonly Mock<IStorageProvider> _storageProviderMock;

  private readonly Mock<IUnitOfWork> _uowMock;

  private readonly Mock<ResiliencePipelineProvider<string>> _pipeline;

  public UpdateAvatarCommandHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _storageProviderMock = new Mock<IStorageProvider>();
    _uowMock = new Mock<IUnitOfWork>();
    _pipeline = new Mock<ResiliencePipelineProvider<string>>();

    var logger = new Mock<ILogger<UpdateAvatarCommandHandler>>();

    _handler = new UpdateAvatarCommandHandler(
      _repositoryMock.Object,
      _storageProviderMock.Object,
      _pipeline.Object,
      logger.Object,
      _uowMock.Object);
  }

  [Fact]
  public async Task Handle_WhenUserDoesNotExist_ShouldReturnNotFound()
  {
    // Arrange
    var userId = Guid.NewGuid();
    IFormFile file = CreateFormFile();

    var command = new UpdateAvatarCommand(userId, file);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(userId, CancellationToken.None))
      .ReturnsAsync((User?)null);

    // Act
    Result<UpdateAvatarResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    result.Error.Description.Should().Be("User not registered");

    _storageProviderMock.Verify(
      x => x.UploadImage(
        It.IsAny<IFormFile>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.AddUserAvatar(
        It.IsAny<UserAvatar>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.UpdateUserAvatar(
        It.IsAny<UserAvatar>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenUserHasNoAvatar_ShouldCreateAvatar()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    IFormFile file = CreateFormFile();

    var command = new UpdateAvatarCommand(user.Id, file);

    var uploadedImage = new StorageUploadResponseDto(
      "https://storage.test/avatars/new-avatar.jpg",
      "avatars/new-avatar.jpg",
      "image/jpeg");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _storageProviderMock
      .Setup(x => x.UploadImage(file, CancellationToken.None))
      .ReturnsAsync(uploadedImage);

    _repositoryMock
      .Setup(x => x.UpdateAsync(
        It.IsAny<User>(),
        CancellationToken.None));

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ReturnsAsync(0);

    // Act
    Result<UpdateAvatarResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.status.Should().Be("created");

    _storageProviderMock.Verify(
      x => x.UploadImage(file, CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.UpdateAsync(
        It.IsAny<User>(),
        It.IsAny<CancellationToken>()),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(CancellationToken.None),
      Times.Once);

    _storageProviderMock.Verify(
      x => x.DeleteImage(
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenUserAlreadyHasAvatar_ShouldUpdateAvatar()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    user.SetAvatar(
      "https://storage.test/avatars/old-avatar.jpg",
      "old-avatar.jpg",
      "image/jpeg");

    IFormFile file = CreateFormFile();

    var command = new UpdateAvatarCommand(user.Id, file);

    var uploadedImage = new StorageUploadResponseDto(
      "https://storage.test/avatars/new-avatar.jpg",
      "new-avatar.jpg",
      "image/jpeg");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _storageProviderMock
      .Setup(x => x.UploadImage(file, CancellationToken.None))
      .ReturnsAsync(uploadedImage);

    _repositoryMock
      .Setup(x => x.UpdateAsync(
        user,
        CancellationToken.None));

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ReturnsAsync(0);

    _storageProviderMock
      .Setup(x => x.DeleteImage(
        "old-avatar.jpg",
        CancellationToken.None))
      .ReturnsAsync(true);

    // Act
    Result<UpdateAvatarResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.status.Should().Be("updated");

    user.UserAvatar!.AvatarImageName
      .Should()
      .Be("new-avatar.jpg");

    _storageProviderMock.Verify(
      x => x.UploadImage(file, CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.UpdateAsync(
        user,
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.AddUserAvatar(
        It.IsAny<UserAvatar>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(CancellationToken.None),
      Times.Once);

    _storageProviderMock.Verify(
      x => x.DeleteImage(
        "old-avatar.jpg",
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenSaveChangesFails_ShouldRollbackUploadedImage()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    IFormFile file = CreateFormFile();

    var command = new UpdateAvatarCommand(user.Id, file);

    var uploadedImage = new StorageUploadResponseDto(
      "avatars/new-avatar.jpg",
      "https://storage.test/avatars/new-avatar.jpg",
      "image/jpeg");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _storageProviderMock
      .Setup(x => x.UploadImage(file, CancellationToken.None))
      .ReturnsAsync(uploadedImage);

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ThrowsAsync(new InvalidOperationException("Database failure"));

    _storageProviderMock
      .Setup(x => x.DeleteImage(
        uploadedImage.Name,
        CancellationToken.None))
      .ReturnsAsync(true);

    // Act
    Result<UpdateAvatarResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.Description
      .Should()
      .Be("Failed to update avatar. Please try again.");

    _storageProviderMock.Verify(
      x => x.UploadImage(file, CancellationToken.None),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(CancellationToken.None),
      Times.Once);

    _storageProviderMock.Verify(
      x => x.DeleteImage(
        uploadedImage.Name,
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenDeletePreviousImageFails_ShouldStillReturnSuccess()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    user.SetAvatar("https://storage.test/avatars/old-avatar.jpg",
      "old-avatar.jpg",
      "image/jpeg");

    IFormFile file = CreateFormFile();

    var command = new UpdateAvatarCommand(user.Id, file);

    var uploadedImage = new StorageUploadResponseDto(
      "https://storage.test/avatars/new-avatar.jpg",
      "avatars/new-avatar.jpg",
      "image/jpeg");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _storageProviderMock
      .Setup(x => x.UploadImage(file, CancellationToken.None))
      .ReturnsAsync(uploadedImage);

    _repositoryMock
      .Setup(x => x.UpdateAsync(
        It.IsAny<User>(),
        CancellationToken.None));

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ReturnsAsync(1);

    _storageProviderMock
      .Setup(x => x.DeleteImage(
        "old-avatar.jpg",
        CancellationToken.None))
      .ThrowsAsync(new InvalidOperationException("Storage unavailable"));

    // Act
    Result<UpdateAvatarResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.status.Should().Be("updated");

    _storageProviderMock.Verify(
      x => x.UploadImage(
        file,
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.UpdateAsync(
        It.IsAny<User>(),
        CancellationToken.None),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(CancellationToken.None),
      Times.Once);

    _storageProviderMock.Verify(
      x => x.DeleteImage(
        "old-avatar.jpg",
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenRollbackFails_ShouldReturnFailure()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    IFormFile file = CreateFormFile();

    var command = new UpdateAvatarCommand(user.Id, file);

    var uploadedImage = new StorageUploadResponseDto(
      "avatars/new-avatar.jpg",
      "https://storage.test/avatars/new-avatar.jpg",
      "image/jpeg");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _storageProviderMock
      .Setup(x => x.UploadImage(file, CancellationToken.None))
      .ReturnsAsync(uploadedImage);

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ThrowsAsync(new InvalidOperationException("Database failure"));

    _storageProviderMock
      .Setup(x => x.DeleteImage(
        uploadedImage.Name,
        CancellationToken.None))
      .ThrowsAsync(new InvalidOperationException("Storage unavailable"));

    // Act
    Result<UpdateAvatarResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.Description
      .Should()
      .Be("Failed to update avatar. Please try again.");

    _storageProviderMock.Verify(
      x => x.DeleteImage(
        uploadedImage.Name,
        CancellationToken.None),
      Times.Once);
  }

  private static IFormFile CreateFormFile()
  {
    var stream = new MemoryStream("fake image content"u8.ToArray());

    return new FormFile(
      stream,
      0,
      stream.Length,
      "file",
      "avatar.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
  }
}
