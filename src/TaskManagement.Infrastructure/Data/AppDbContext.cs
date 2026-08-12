using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskStatus = TaskManagement.Core.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- User ------------------------------------------------------------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();

            // Store the role as a readable string ("Admin"/"User") to match the API DTOs.
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasMany(u => u.AssignedTasks)
                  .WithOne(t => t.AssignedUser)
                  .HasForeignKey(t => t.AssignedUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // --- Category --------------------------------------------------------------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.HasMany(c => c.Tasks)
                  .WithOne(t => t.Category)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Task ------------------------------------------------------------------
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(2000);

            // Store enums as readable strings ("Pending", "High", ...) to match the API DTOs.
            entity.Property(t => t.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(t => t.Priority)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.AssignedUserId);
        });
    }
}
