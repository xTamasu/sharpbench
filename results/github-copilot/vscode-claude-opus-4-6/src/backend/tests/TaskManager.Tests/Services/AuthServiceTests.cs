// Unit tests for AuthService: covers registration, login, duplicate email, and wrong password.
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();

        // Set up in-memory configuration for JWT settings
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "TestSecretKeyThatIsLongEnoughForHmacSha256Algorithm!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpiryInMinutes", "60" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _sut = new AuthService(_userRepoMock.Object, _configuration);
    }

    [Fact]
    public async Task RegisterAsync_HappyPath_ReturnsTokenAndUserInfo()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            DisplayName = "Test User"
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.DisplayName, result.DisplayName);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123",
            DisplayName = "Test"
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User { Email = request.Email });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_HappyPath_ReturnsTokenAndUserInfo()
    {
        // Arrange
        var password = "Password123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DisplayName = "Test User"
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var request = new LoginRequest { Email = user.Email, Password = password };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            DisplayName = "Test"
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var request = new LoginRequest { Email = user.Email, Password = "WrongPassword" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync("nobody@example.com"))
            .ReturnsAsync((User?)null);

        var request = new LoginRequest { Email = "nobody@example.com", Password = "Password123" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(request));
    }
}
