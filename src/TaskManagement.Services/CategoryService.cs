using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.DTOs;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Services;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetCategoriesAsync();
}

/// <summary>
/// Categories are read-only for now (they are seeded on startup) — the UI
/// only needs the list for the category dropdown and badges.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync()
        => await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CategoryResponse { Id = c.Id, Name = c.Name })
            .ToListAsync();
}
