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
// Both actions live on "users" and share the auth/user services — splitting
// them into two controllers would add boilerplate without real benefit (S6960).
#pragma warning disable S6960 // Controllers should not have too many responsibilities
public class UsersController : ControllerBase
#pragma warning restore S6960
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public UsersController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
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

    /// <summary>
    /// GET /api/users — the user directory for the "Assigned To" dropdown.
    /// Admins only, because only admins can assign tasks to others.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
        => Ok(await _userService.GetUsersAsync());
}
