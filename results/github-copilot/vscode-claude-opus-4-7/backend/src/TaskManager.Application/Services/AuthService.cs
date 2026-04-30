// AuthService.cs
// Implements registration and login: validates input, hashes/verifies passwords
// via IPasswordHasher, persists the user via IUserRepository, and issues a JWT.
using FluentValidation;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Mapping;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IUserRepository users,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // Validate the incoming DTO before touching the database.
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationFailedException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Reject duplicate registrations (email is the natural key for login).
        if (await _users.GetByEmailAsync(normalizedEmail, ct) is not null)
            throw new ConflictException("A user with that email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        return new AuthResponse(_jwt.GenerateToken(user), user.ToDto());
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationFailedException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        // Use the same error for unknown email and wrong password to avoid user enumeration.
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new AuthenticationFailedException("Invalid email or password.");

        return new AuthResponse(_jwt.GenerateToken(user), user.ToDto());
    }
}
