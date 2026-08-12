namespace TaskManagement.Core.DTOs;

/// <summary>
/// The JSON body the client sends to POST /api/auth/register.
/// </summary>
public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
