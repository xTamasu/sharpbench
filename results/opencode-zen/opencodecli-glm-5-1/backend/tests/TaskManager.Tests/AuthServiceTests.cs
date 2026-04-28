// Unit tests for AuthService covering registration, login, and edge cases
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtTokenGenerator>();
        _sut = new AuthService(_userRepoMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns("token");

        var request = new RegisterRequest("test@test.com", "password123", "Test User");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("Test User", result.DisplayName);
        Assert.Equal("token", result.Token);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsBadRequestException()
    {
        // Arrange
        var existing = new User { Email = "test@test.com" };
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(existing);

        var request = new RegisterRequest("test@test.com", "password123", "Test User");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            DisplayName = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns("token");

        var request = new LoginRequest("test@test.com", "password123");

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("token", result.Token);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsBadRequestException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync("notfound@test.com")).ReturnsAsync((User?)null);

        var request = new LoginRequest("notfound@test.com", "password123");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        var request = new LoginRequest("test@test.com", "wrongpassword");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.LoginAsync(request));
    }
}