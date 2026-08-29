using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.Application.Features.Users.Register;
using Identity.Tests.Api.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Tests.Api.Endpoints.Users.Register;

public sealed class RegisterUserEndpointTests : IClassFixture<IdentityApiFactory>
{
  private readonly HttpClient _client;
  private const string BaseUrl = "/api/v1/identity/register";

  public RegisterUserEndpointTests(IdentityApiFactory factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Register_ShouldNotRequireAuthentication()
  {
    // Arrange
    var request = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = $"joao.{Guid.NewGuid():N}@example.com",
      Password = "Password@123!"
    };

    // Act
    HttpResponseMessage response =
      await _client.PostAsJsonAsync(BaseUrl, request);

    string body =
      await response.Content.ReadAsStringAsync();

    // Debug
    Console.WriteLine($"Status: {response.StatusCode}");
    Console.WriteLine($"Body: {body}");

    // Assert
    response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized, body);
  }

  [Fact]
  public async Task Register_WithValidRequest_ShouldReturnCreated()
  {
    // Arrange
    var request = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = $"joao.{Guid.NewGuid():N}@example.com",
      Password = "Password@123!"
    };

    // Act
    HttpResponseMessage response =
      await _client.PostAsJsonAsync(BaseUrl, request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    RegisterUserResponse? result =
      await response.Content.ReadFromJsonAsync<RegisterUserResponse>();

    result.Should().NotBeNull();
    result!.Id.Should().NotBeEmpty();

    response.Headers.Location.Should().NotBeNull();
    response.Headers.Location!.ToString()
      .Should()
      .Be($"/{result.Id}");
  }

  [Fact]
  public async Task Register_WithExistingEmail_ShouldReturnConflict()
  {
    // Arrange
    string email = $"existing.{Guid.NewGuid():N}@example.com";

    var request = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = email,
      Password = "Password@123!"
    };

    HttpResponseMessage firstResponse =
      await _client.PostAsJsonAsync(BaseUrl, request);

    firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    // Act
    HttpResponseMessage secondResponse =
      await _client.PostAsJsonAsync(BaseUrl, request);

    // Assert
    secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

    ProblemDetails? problem =
      await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    problem.Should().NotBeNull();
    problem!.Detail.Should().Be("User already registered!");
    problem.Title.Should().Be("Error.Conflict");
    problem.Status.Should().Be(StatusCodes.Status409Conflict);
  }

  [Fact]
  public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
  {
    // Arrange
    var request = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = "invalid-email",
      Password = "Password@123!"
    };

    // Act
    HttpResponseMessage response =
      await _client.PostAsJsonAsync(BaseUrl, request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithEmptyEmail_ShouldReturnBadRequest()
  {
    // Arrange
    var request = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = string.Empty,
      Password = "Password@123!"
    };

    // Act
    HttpResponseMessage response =
      await _client.PostAsJsonAsync(BaseUrl, request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithShortPassword_ShouldReturnBadRequest()
  {
    // Arrange
    var request = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = $"joao.{Guid.NewGuid():N}@example.com",
      Password = "1234567"
    };

    // Act
    HttpResponseMessage response =
      await _client.PostAsJsonAsync(BaseUrl, request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithEmptyPassword_ShouldReturnBadRequest()
  {
    // Arrange
    var request = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = $"joao.{Guid.NewGuid():N}@example.com",
      Password = string.Empty
    };

    // Act
    HttpResponseMessage response =
      await _client.PostAsJsonAsync(BaseUrl, request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithValidRequest_ShouldReturnUserInformation()
  {
    // Arrange
    var request = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = $"joao.{Guid.NewGuid():N}@example.com",
      Password = "Password@123!"
    };

    // Act
    HttpResponseMessage response =
      await _client.PostAsJsonAsync(BaseUrl, request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    RegisterUserResponse? result =
      await response.Content.ReadFromJsonAsync<RegisterUserResponse>();

    result.Should().NotBeNull();
    result!.Id.Should().NotBeEmpty();
  }

  [Fact]
  public async Task Register_WithDifferentEmails_ShouldCreateDifferentUsers()
  {
    // Arrange
    var firstRequest = new
    {
      FirstName = "João",
      LastName = "Silva",
      Email = $"joao.{Guid.NewGuid():N}@example.com",
      Password = "Password@123!"
    };

    var secondRequest = new
    {
      FirstName = "Maria",
      LastName = "Santos",
      Email = $"maria.{Guid.NewGuid():N}@example.com",
      Password = "Password@123!"
    };

    // Act
    HttpResponseMessage firstResponse =
      await _client.PostAsJsonAsync(BaseUrl, firstRequest);

    HttpResponseMessage secondResponse =
      await _client.PostAsJsonAsync(BaseUrl, secondRequest);

    // Assert
    firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    RegisterUserResponse? firstUser =
      await firstResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

    RegisterUserResponse? secondUser =
      await secondResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

    firstUser.Should().NotBeNull();
    secondUser.Should().NotBeNull();

    firstUser!.Id.Should().NotBe(secondUser!.Id);
  }
}

