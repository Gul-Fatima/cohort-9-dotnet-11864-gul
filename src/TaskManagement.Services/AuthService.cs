using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Exceptions;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserResponse?> GetUserByIdAsync(int id);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // --- Register --------------------------------------------------------------
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate before touching the database (matches the mock API's 400s).
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ApiException(400, "Validation failed", "Name, email and password are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();

        // Business rule: one account per email.
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            throw new ApiException(409, "Registration failed", "An account with this email already exists.");
        }

        // BCrypt hashes the password with a random salt, so we NEVER store
        // the plain-text password. Two hashes of the same password differ.
        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User // every self-registered account is a normal user
        };

        _context.Users.Add(user);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two requests raced to register the same email: the unique index
            // rejected the second insert. Surface it as 409, not a 500.
            throw new ApiException(409, "Registration failed", "An account with this email already exists.");
        }

        return new AuthResponse { Token = GenerateToken(user), User = ToResponse(user) };
    }

    // --- Login ------------------------------------------------------------------
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ApiException(400, "Validation failed", "Email and password are required.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant());

        // Same message for "no such user" and "wrong password" — don't leak
        // which one it was. BCrypt.Verify re-hashes the input and compares.
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new ApiException(401, "Login failed", "Invalid email or password.");
        }

        return new AuthResponse { Token = GenerateToken(user), User = ToResponse(user) };
    }

    // --- Current user ------------------------------------------------------------
    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    // --- JWT ---------------------------------------------------------------------
    /// <summary>
    /// Signs a JWT containing the user's identity so the server can trust it
    /// later WITHOUT hitting the database on every request.
    /// </summary>
    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims are name/value facts about the user baked into the token.
        // "sub" = subject (the user id), role is what [Authorize(Roles=...)] reads.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],                       // who issued it
            audience: _config["Jwt:Audience"],                   // who it's for
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: credentials);                    // the HMAC signature

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponse ToResponse(User u)
    {
        return new UserResponse
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role.ToString(),
            CreatedAt = u.CreatedAt
        };
    }
}
