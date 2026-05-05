// AuthService — handles user registration and login with BCrypt password hashing and JWT generation.

using System;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IRepository<User> userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (existing != null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Id = user.Id
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Id = user.Id
        };
    }
}
