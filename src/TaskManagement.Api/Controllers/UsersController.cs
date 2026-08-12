using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;

namespace TaskManagement.Api.Controllers;

/// <summary>
/// User endpoints. [Authorize] means a valid Bearer token is required.
/// </summary>
[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// GET /api/users/me — returns the currently logged-in user.
    /// The user id comes from the "sub" claim in the validated JWT,
    /// not from a parameter, so nobody can ask about someone else.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // FindFirstValue reads a claim out of the token the server just validated.
        // TryParse keeps a malformed token from turning into a 500.
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
        {
            return Unauthorized(new { title = "Unauthorized", message = "Invalid token." });
        }

        var user = await _authService.GetUserByIdAsync(userId);
        return user is null
            ? NotFound(new { title = "Not found", message = "User not found." })
            : Ok(user);
    }
}
