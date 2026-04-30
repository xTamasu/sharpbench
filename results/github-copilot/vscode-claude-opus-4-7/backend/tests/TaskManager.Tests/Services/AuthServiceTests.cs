// AuthServiceTests.cs
// Unit tests for AuthService covering register and login happy + failure paths.
using FluentValidation;
using Moq;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Application.Validation;
using TaskManager.Domain.Entities;

namespace TaskManager.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwt = new();
    private readonly IValidator<RegisterRequest> _registerValidator = new RegisterRequestValidator();
    private readonly IValidator<LoginRequest> _loginValidator = new LoginRequestValidator();

    private AuthService Build() =>
        new(_users.Object, _hasher.Object, _jwt.Object, _registerValidator, _loginValidator);

    [Fact]
    public async Task Register_HappyPath_PersistsUserAndReturnsToken()
    {
        // Arrange
        var req = new RegisterRequest("New@Example.com", "Password123!", "New User");
        _users.Setup(r => r.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash("Password123!")).Returns("hashed");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        // Act
        var result = await Build().RegisterAsync(req);

        // Assert
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("new@example.com", result.User.Email);
        _users.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "new@example.com" && u.PasswordHash == "hashed"), It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsConflict()
    {
        // Arrange
        var req = new RegisterRequest("dup@example.com", "Password123!", "Dup");
        _users.Setup(r => r.GetByEmailAsync("dup@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(new User { Email = "dup@example.com" });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => Build().RegisterAsync(req));
    }

    [Fact]
    public async Task Register_InvalidEmail_ThrowsValidationFailed()
    {
        // Arrange
        var req = new RegisterRequest("not-an-email", "Password123!", "Dn");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationFailedException>(() => Build().RegisterAsync(req));
    }

    [Fact]
    public async Task Register_ShortPassword_ThrowsValidationFailed()
    {
        var req = new RegisterRequest("ok@example.com", "short", "Dn");
        await Assert.ThrowsAsync<ValidationFailedException>(() => Build().RegisterAsync(req));
    }

    [Fact]
    public async Task Login_HappyPath_ReturnsToken()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "u@example.com", PasswordHash = "hash", DisplayName = "U" };
        _users.Setup(r => r.GetByEmailAsync("u@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("pw", "hash")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("tok");

        // Act
        var result = await Build().LoginAsync(new LoginRequest("u@example.com", "pw"));

        // Assert
        Assert.Equal("tok", result.Token);
        Assert.Equal(user.Id, result.User.Id);
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsAuthenticationFailed()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => Build().LoginAsync(new LoginRequest("missing@example.com", "x")));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsAuthenticationFailed()
    {
        var user = new User { Email = "u@example.com", PasswordHash = "h" };
        _users.Setup(r => r.GetByEmailAsync("u@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("bad", "h")).Returns(false);

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => Build().LoginAsync(new LoginRequest("u@example.com", "bad")));
    }
}
