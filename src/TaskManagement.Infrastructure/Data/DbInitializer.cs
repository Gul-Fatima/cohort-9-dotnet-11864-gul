using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;

namespace TaskManagement.Infrastructure.Data;

public static class DbInitializer
{
    /// <summary>
    /// Seeds default categories and an admin account if the tables are empty.
    /// Called once at startup after migrations are applied.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Work" },
                new Category { Name = "Personal" },
                new Category { Name = "Study" },
                new Category { Name = "Urgent" });
        }

        if (!await context.Users.AnyAsync())
        {
            context.Users.Add(new User
            {
                Name = "Admin",
                Email = "admin@taskmgmt.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            });
        }

        await context.SaveChangesAsync();
    }
}
