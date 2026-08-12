namespace TaskManagement.Core.DTOs;

/// <summary>
/// What login/register return: a signed JWT plus the public user info.
/// Matches the shape the frontend mock already produces.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public UserResponse User { get; set; } = new();
}
