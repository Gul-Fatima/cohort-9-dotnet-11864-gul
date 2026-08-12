namespace TaskManagement.Core.DTOs;

/// <summary>
/// The JSON body the client sends to POST /api/auth/login.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
