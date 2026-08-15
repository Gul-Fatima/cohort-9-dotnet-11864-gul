namespace TaskManagement.Core.DTOs;

/// <summary>
/// A task as the API exposes it — decorated with the category name and the
/// assignee so the frontend can render directly. Matches the mock API shape.
/// </summary>
public class TaskResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public int? CategoryId { get; set; }

    public string Category { get; set; } = "Uncategorized";

    public int? AssignedUserId { get; set; }

    public AssignedToResponse? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// The minimal assignee info the UI shows next to a task ("Assigned to X").
/// </summary>
public class AssignedToResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
