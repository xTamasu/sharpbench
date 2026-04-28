using BCrypt.Net;
using Moq;
using TaskManager.Application.Dto.Auth;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.Services;

/// <summary>
/// Unit tests for AuthService.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _authService = new AuthService(_userRepositoryMock.Object, "test_secret_key_that_is_very_long", "TestIssuer", "TestAudience");
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterAsync_WithValidData_CreatesUserAndReturnsToken()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@example.com", Password = "Password123", DisplayName = "Test User" };
        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("test@example.com", result.User.Email);
        Assert.Equal("Test User", result.User.DisplayName);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@example.com", Password = "Password123", DisplayName = "Test User" };
        var existingUser = new User { Email = "test@example.com", Id = Guid.NewGuid() };
        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { existingUser });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
    }

    [Fact]
    public async System.Threading.Tasks.Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var password = "Password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = hashedPassword, DisplayName = "Test User" };
        var request = new LoginRequest { Email = "test@example.com", Password = password };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { user });

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal(user.Email, result.User.Email);
    }

    [Fact]
    public async System.Threading.Tasks.Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = hashedPassword };
        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { user });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async System.Threading.Tasks.Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest { Email = "nonexistent@example.com", Password = "Password123" };
        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";

        // Act
        var token = _authService.GenerateAccessToken(userId, email);

        // Assert
        Assert.NotEmpty(token);
        Assert.StartsWith("eyJ", token);
    }
}
