// AuthController.cs
// Handles user registration and login. Both endpoints are anonymous.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
        => Ok(await _auth.RegisterAsync(request, ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
        => Ok(await _auth.LoginAsync(request, ct));
}
