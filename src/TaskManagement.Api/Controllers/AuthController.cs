using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTOs;
using TaskManagement.Services;

namespace TaskManagement.Api.Controllers;

/// <summary>
/// Public endpoints for creating an account and exchanging credentials for a JWT.
/// Errors (400/401/409) are converted to { title, message } by the global
/// ApiExceptionFilter — no try/catch needed here.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        => Ok(await _authService.RegisterAsync(request)); // 200 + { token, user }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => Ok(await _authService.LoginAsync(request)); // 200 + { token, user }
}
