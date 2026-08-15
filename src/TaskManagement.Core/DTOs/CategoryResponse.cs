namespace TaskManagement.Core.DTOs;

/// <summary>
/// A category as the API exposes it — just the id and name the UI needs
/// for the category dropdown and task badges.
/// </summary>
public class CategoryResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
