using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Infrastructure.Data;
using TaskStatus = TaskManagement.Core.Enums.TaskStatus;

namespace TaskManagement.Tests;

/// <summary>
/// Builds an isolated in-memory AppDbContext per test with a known seed:
/// 2 categories, an Admin (id 1) + a regular User (id 2), and 2 tasks —
/// one assigned to each. Every test gets a fresh Guid database, so tests
/// never share state.
/// </summary>
public static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        Seed(context);
        return context;
    }

    private static void Seed(AppDbContext context)
    {
        context.Categories.AddRange(
            new Category { Id = 1, Name = "Work" },
            new Category { Id = 2, Name = "Personal" });

        context.Users.AddRange(
            new User
            {
                Id = 1,
                Name = "Admin",
                Email = "admin@taskmgmt.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            },
            new User
            {
                Id = 2,
                Name = "Regular User",
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = UserRole.User
            });

        context.Tasks.AddRange(
            new TaskItem
            {
                Id = 1,
                Title = "Admin's task",
                Description = "assigned to the admin",
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                CategoryId = 1,
                AssignedUserId = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new TaskItem
            {
                Id = 2,
                Title = "User's task",
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Low,
                CategoryId = 2,
                AssignedUserId = 2,
                CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            });

        context.SaveChanges();
    }
}
