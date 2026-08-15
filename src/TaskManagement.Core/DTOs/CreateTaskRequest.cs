namespace TaskManagement.Core.DTOs;

/// <summary>
/// Body for POST /api/tasks. Status/Priority/DueDate are strings so the
/// service can validate them and produce friendly 400s, mirroring the mock.
/// </summary>
public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    /// <summary>"yyyy-MM-dd" / ISO string, or null/"" for no due date.</summary>
    public string? DueDate { get; set; }

    public int? CategoryId { get; set; }

    public int? AssignedUserId { get; set; }
}
