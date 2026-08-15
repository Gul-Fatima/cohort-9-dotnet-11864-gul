using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.DTOs;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync();
}

/// <summary>
/// Admin-only user directory — used to populate the "Assigned To" dropdown
/// on the task form. Never exposes password hashes.
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserResponse>> GetUsersAsync()
        => await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
}
