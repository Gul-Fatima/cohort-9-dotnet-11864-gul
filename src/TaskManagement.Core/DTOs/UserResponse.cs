namespace TaskManagement.Core.DTOs;

/// <summary>
/// A user as the API exposes it. Never includes the password hash.
/// </summary>
public class UserResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
