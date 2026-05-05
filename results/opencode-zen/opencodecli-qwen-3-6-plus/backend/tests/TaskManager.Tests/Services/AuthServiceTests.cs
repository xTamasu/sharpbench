// AuthServiceTests — tests for registration and login including duplicate email and invalid credentials.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockJwtService = new Mock<IJwtService>();
        _service = new AuthService(_mockUserRepo.Object, _mockJwtService.Object);
    }

    [Fact]
    public async Task RegisterAsync_HappyPath_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            DisplayName = "Test User"
        };
        var token = "fake-jwt-token";
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(token);

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.DisplayName.Should().Be(request.DisplayName);
        result.Token.Should().Be(token);
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123",
            DisplayName = "Test User"
        };
        _mockUserRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = request.Email });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_HappyPath_ReturnsAuthResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        var token = "fake-jwt-token";

        _mockUserRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockJwtService.Setup(j => j.GenerateToken(user)).Returns(token);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };
        _mockUserRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        _mockUserRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(request));
    }
}
