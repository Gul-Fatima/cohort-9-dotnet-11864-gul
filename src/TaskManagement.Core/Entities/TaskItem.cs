using TaskManagement.Core.Enums;

// Alias avoids ambiguity with System.Threading.Tasks.TaskStatus (implicit using).
using TaskStatus = TaskManagement.Core.Enums.TaskStatus;

namespace TaskManagement.Core.Entities;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    // Nullable so tasks can be "Uncategorized" (the UI offers that option).
    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    public int? AssignedUserId { get; set; }

    public User? AssignedUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
