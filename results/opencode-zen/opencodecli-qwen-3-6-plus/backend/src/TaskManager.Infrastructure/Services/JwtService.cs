// JwtService — generates JWT tokens using configuration values for secret, issuer, audience, and expiry.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var secret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured");
        var issuer = _config["Jwt:Issuer"] ?? "TaskManagerApi";
        var audience = _config["Jwt:Audience"] ?? "TaskManagerClient";
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
