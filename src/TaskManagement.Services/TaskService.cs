using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Exceptions;
using TaskManagement.Infrastructure.Data;
using TaskStatus = TaskManagement.Core.Enums.TaskStatus;

namespace TaskManagement.Services;

public interface ITaskService
{
    Task<List<TaskResponse>> GetTasksAsync(TaskQuery query, int viewerId, bool isAdmin);
    Task<TaskResponse> GetTaskAsync(int id, int viewerId, bool isAdmin);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, int creatorId);
    Task<TaskResponse> UpdateTaskAsync(int id, UpdateTaskRequest request, int viewerId, bool isAdmin);
    Task DeleteTaskAsync(int id, int viewerId, bool isAdmin);
    Task<DashboardStatsResponse> GetDashboardStatsAsync(int viewerId, bool isAdmin);
}

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    // --- List (with filters + role scoping) ------------------------------------
    public async Task<List<TaskResponse>> GetTasksAsync(TaskQuery query, int viewerId, bool isAdmin)
    {
        var q = _context.Tasks.AsNoTracking().AsQueryable();

        // Business rule (mirrors the mock): admins see every task,
        // regular users only see the tasks assigned to them.
        if (!isAdmin)
        {
            q = q.Where(t => t.AssignedUserId == viewerId);
        }

        // --- optional filters ---
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<TaskStatus>(query.Status, out var status))
        {
            q = q.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Priority) && Enum.TryParse<TaskPriority>(query.Priority, out var priority))
        {
            q = q.Where(t => t.Priority == priority);
        }

        if (query.CategoryId.HasValue)
        {
            q = q.Where(t => t.CategoryId == query.CategoryId.Value);
        }

        if (query.AssignedUserId.HasValue)
        {
            q = q.Where(t => t.AssignedUserId == query.AssignedUserId.Value);
        }

        if (query.DueDate.HasValue)
        {
            q = q.Where(t => t.DueDate != null && t.DueDate.Value.Date == query.DueDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(t =>
                t.Title.ToLower().Contains(search) ||
                (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        // Same ordering as the mock: Pending first, then InProgress, then
        // Completed, and within each status the earliest due date first.
        // The projection is written inline (not a method call) so EF Core can
        // translate it to SQL and join the category + assignee automatically.
        // (The nested ternary is intentional: it must stay inline to translate.)
#pragma warning disable S3358 // Extract this nested ternary into an independent statement
        return await q
            .OrderBy(t => t.Status == TaskStatus.Pending ? 0
                        : t.Status == TaskStatus.InProgress ? 1
                        : 2)
            .ThenBy(t => t.DueDate)
#pragma warning restore S3358
            .Select(t => new TaskResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                DueDate = t.DueDate,
                CategoryId = t.CategoryId,
                Category = t.Category != null ? t.Category.Name : "Uncategorized",
                AssignedUserId = t.AssignedUserId,
                AssignedTo = t.AssignedUser != null
                    ? new AssignedToResponse { Id = t.AssignedUser.Id, Name = t.AssignedUser.Name }
                    : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();
    }

    // --- Get one ----------------------------------------------------------------
    public async Task<TaskResponse> GetTaskAsync(int id, int viewerId, bool isAdmin)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        // Same scoping as the list: a regular user asking about someone
        // else's task gets 404 (don't leak whether the task exists).
        if (task is null || (!isAdmin && task.AssignedUserId != viewerId))
        {
            throw new ApiException(404, "Not found", "Task not found.");
        }

        return ToResponse(task);
    }

    // --- Create ------------------------------------------------------------------
    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, int creatorId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ApiException(400, "Validation failed", "Title is required.");
        }

        // Validate enum strings before saving so a typo is a 400, not a 500.
        if (request.Status is not null && !Enum.TryParse<TaskStatus>(request.Status, out _))
        {
            throw new ApiException(400, "Validation failed", "Invalid status.");
        }
        if (request.Priority is not null && !Enum.TryParse<TaskPriority>(request.Priority, out _))
        {
            throw new ApiException(400, "Validation failed", "Invalid priority.");
        }

        await EnsureCategoryExistsAsync(request.CategoryId);
        await EnsureUserExistsAsync(request.AssignedUserId);

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Status = ParseOr(request.Status, TaskStatus.Pending),
            Priority = ParseOr(request.Priority, TaskPriority.Medium),
            DueDate = ParseDueDate(request.DueDate),
            CategoryId = request.CategoryId,
            // Unassigned tasks default to the creator (matches the mock).
            AssignedUserId = request.AssignedUserId ?? creatorId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return await GetTaskAsync(task.Id, creatorId, isAdmin: true);
    }

    // --- Update --------------------------------------------------------------------
    public async Task<TaskResponse> UpdateTaskAsync(int id, UpdateTaskRequest request, int viewerId, bool isAdmin)
    {
        var task = await _context.Tasks
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
        {
            throw new ApiException(404, "Not found", "Task not found.");
        }

        // Regular users may only edit their own tasks (mirrors the mock's 403).
        if (!isAdmin && task.AssignedUserId != viewerId)
        {
            throw new ApiException(403, "Forbidden", "You can only edit your own tasks.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            task.Title = request.Title.Trim();
        }
        if (request.Description is not null)
        {
            task.Description = request.Description.Trim();
        }
        if (request.Status is not null)
        {
            if (!Enum.TryParse<TaskStatus>(request.Status, out var status))
            {
                throw new ApiException(400, "Validation failed", "Invalid status.");
            }
            task.Status = status;
        }
        if (request.Priority is not null)
        {
            if (!Enum.TryParse<TaskPriority>(request.Priority, out var priority))
            {
                throw new ApiException(400, "Validation failed", "Invalid priority.");
            }
            task.Priority = priority;
        }
        if (request.DueDate is not null)
        {
            task.DueDate = ParseDueDate(request.DueDate);
        }
        if (request.CategoryId is not null)
        {
            await EnsureCategoryExistsAsync(request.CategoryId);
            task.CategoryId = request.CategoryId;
        }
        if (request.AssignedUserId is not null)
        {
            await EnsureUserExistsAsync(request.AssignedUserId);
            task.AssignedUserId = request.AssignedUserId;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ToResponse(task);
    }

    // --- Delete ---------------------------------------------------------------------
    public async Task DeleteTaskAsync(int id, int viewerId, bool isAdmin)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
        {
            throw new ApiException(404, "Not found", "Task not found.");
        }

        if (!isAdmin && task.AssignedUserId != viewerId)
        {
            throw new ApiException(403, "Forbidden", "You can only delete your own tasks.");
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    // --- Dashboard stats --------------------------------------------------------------
    public async Task<DashboardStatsResponse> GetDashboardStatsAsync(int viewerId, bool isAdmin)
    {
        var q = _context.Tasks.AsNoTracking().AsQueryable();

        if (!isAdmin)
        {
            q = q.Where(t => t.AssignedUserId == viewerId);
        }

        var tasks = await q.Select(t => t.Status).ToListAsync();

        var stats = new DashboardStatsResponse { Total = tasks.Count };
        foreach (var status in tasks)
        {
            if (status == TaskStatus.Completed) stats.Completed++;
            else if (status == TaskStatus.InProgress) stats.InProgress++;
            else stats.Pending++;
        }

        return stats;
    }

    // --- helpers -------------------------------------------------------------------
    private async Task EnsureCategoryExistsAsync(int? categoryId)
    {
        if (categoryId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == categoryId.Value))
        {
            throw new ApiException(400, "Validation failed", "Category not found.");
        }
    }

    private async Task EnsureUserExistsAsync(int? userId)
    {
        if (userId.HasValue && !await _context.Users.AnyAsync(u => u.Id == userId.Value))
        {
            throw new ApiException(400, "Validation failed", "Assigned user not found.");
        }
    }

    private static TaskStatus ParseOr(string? value, TaskStatus fallback)
        => Enum.TryParse<TaskStatus>(value, out var parsed) ? parsed : fallback;

    private static TaskPriority ParseOr(string? value, TaskPriority fallback)
        => Enum.TryParse<TaskPriority>(value, out var parsed) ? parsed : fallback;

    /// <summary>"2026-01-15", ISO strings, or ""/null all work; "" means no due date.</summary>
    private static DateTime? ParseDueDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : throw new ApiException(400, "Validation failed", "Invalid due date.");
    }

    private static TaskResponse ToResponse(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status.ToString(),
        Priority = t.Priority.ToString(),
        DueDate = t.DueDate,
        CategoryId = t.CategoryId,
        Category = t.Category?.Name ?? "Uncategorized",
        AssignedUserId = t.AssignedUserId,
        AssignedTo = t.AssignedUser is null
            ? null
            : new AssignedToResponse { Id = t.AssignedUser.Id, Name = t.AssignedUser.Name },
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
