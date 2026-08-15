namespace TaskManagement.Core.DTOs;

/// <summary>
/// Filters for GET /api/tasks, bound from the query string
/// (e.g. /api/tasks?status=Pending&priority=High&search=pay).
/// Empty/absent values mean "no filter" and are ignored.
/// </summary>
public class TaskQuery
{
    public string? Status { get; set; }

    public string? Priority { get; set; }

    public int? CategoryId { get; set; }

    public int? AssignedUserId { get; set; }

    public string? Search { get; set; }

    public DateTime? DueDate { get; set; }
}
