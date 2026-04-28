using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain;
using Xunit;

namespace TaskManager.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _configMock = new Mock<IConfiguration>();

            _configMock.Setup(c => c["JwtSettings:Secret"]).Returns("ThisIsAVeryLongAndSecureSecretKeyForJwtAuthentication123!");
            _configMock.Setup(c => c["JwtSettings:Issuer"]).Returns("TaskManagerApi");
            _configMock.Setup(c => c["JwtSettings:Audience"]).Returns("TaskManagerClients");
            _configMock.Setup(c => c["JwtSettings:ExpiryMinutes"]).Returns("60");

            _authService = new AuthService(_userRepositoryMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnToken_WhenValidRequest()
        {
            // Arrange
            var request = new RegisterRequest("test@example.com", "password123", "Test User");
            _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User>());

            // Act
            var response = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(response.Token);
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(request.DisplayName, response.DisplayName);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenEmailExists()
        {
            // Arrange
            var request = new RegisterRequest("test@example.com", "password123", "Test User");
            var existingUser = new User { Email = request.Email };
            _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { existingUser });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenValidCredentials()
        {
            // Arrange
            var request = new LoginRequest("test@example.com", "password123");
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), DisplayName = "Test" };
            
            _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });

            // Act
            var response = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(response.Token);
            Assert.Equal(user.Id, response.UserId);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorized_WhenInvalidCredentials()
        {
            // Arrange
            var request = new LoginRequest("test@example.com", "wrongpassword");
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), DisplayName = "Test" };
            
            _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }
    }
}
