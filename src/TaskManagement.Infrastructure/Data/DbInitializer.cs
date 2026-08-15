using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskStatus = TaskManagement.Core.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Data;

public static class DbInitializer
{
    /// <summary>
    /// Seeds default categories, two demo accounts and a few sample tasks so
    /// the app is immediately demoable. Called once at startup after migrations.
    /// Each item is seeded on its own "if missing" check, so a partially
    /// seeded database still gets whatever it is missing.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        // --- Categories ----------------------------------------------------------
        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Work" },
                new Category { Name = "Personal" },
                new Category { Name = "Study" },
                new Category { Name = "Urgent" });
            await context.SaveChangesAsync();
        }

        // --- Demo accounts ---------------------------------------------------------
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@taskmgmt.com");
        if (admin is null)
        {
            admin = new User
            {
                Name = "Admin",
                Email = "admin@taskmgmt.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            };
            context.Users.Add(admin);
        }

        var demoUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "user@example.com");
        if (demoUser is null)
        {
            demoUser = new User
            {
                Name = "Demo User",
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = UserRole.User
            };
            context.Users.Add(demoUser);
        }

        await context.SaveChangesAsync(); // ids are needed for the tasks below

        // --- Sample tasks (only when the board is completely empty) ---------------
        if (!await context.Tasks.AnyAsync())
        {
            var work = await context.Categories.SingleAsync(c => c.Name == "Work");
            var personal = await context.Categories.SingleAsync(c => c.Name == "Personal");
            var study = await context.Categories.SingleAsync(c => c.Name == "Study");
            var urgent = await context.Categories.SingleAsync(c => c.Name == "Urgent");
            var now = DateTime.UtcNow;

            context.Tasks.AddRange(
                new TaskItem
                {
                    Title = "Set up the .NET solution",
                    Description = "API, Core, Infrastructure and Services projects.",
                    Status = TaskStatus.Completed,
                    Priority = TaskPriority.High,
                    DueDate = now.AddDays(-3),
                    CategoryId = work.Id,
                    AssignedUserId = admin.Id,
                    CreatedAt = now.AddDays(-12),
                    UpdatedAt = now.AddDays(-3)
                },
                new TaskItem
                {
                    Title = "Review the frontend pull request",
                    Description = "Check the auth flow and task screens.",
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    DueDate = now.AddDays(2),
                    CategoryId = work.Id,
                    AssignedUserId = admin.Id,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now
                },
                new TaskItem
                {
                    Title = "Write the SonarQube report",
                    Description = "Summarize the code-quality findings.",
                    Status = TaskStatus.Pending,
                    Priority = TaskPriority.Medium,
                    DueDate = now.AddDays(5),
                    CategoryId = study.Id,
                    AssignedUserId = admin.Id,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                },
                new TaskItem
                {
                    Title = "Grocery shopping",
                    Description = "Milk, eggs, bread and coffee.",
                    Status = TaskStatus.Pending,
                    Priority = TaskPriority.Low,
                    DueDate = now.AddDays(2),
                    CategoryId = personal.Id,
                    AssignedUserId = demoUser.Id,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-2)
                },
                new TaskItem
                {
                    Title = "Pay the electricity bill",
                    Description = "Due before the 15th.",
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    DueDate = now.AddDays(1),
                    CategoryId = urgent.Id,
                    AssignedUserId = demoUser.Id,
                    CreatedAt = now.AddDays(-4),
                    UpdatedAt = now.AddDays(-1)
                });

            await context.SaveChangesAsync();
        }
    }
}
