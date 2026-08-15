namespace TaskManagement.Core.DTOs;

/// <summary>
/// Body for PUT /api/tasks/{id}. All fields optional — a null field means
/// "leave it unchanged" (except DueDate: "" clears the due date).
/// </summary>
public class UpdateTaskRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public string? DueDate { get; set; }

    public int? CategoryId { get; set; }

    public int? AssignedUserId { get; set; }
}
