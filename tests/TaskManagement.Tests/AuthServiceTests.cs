using Microsoft.Extensions.Configuration;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Exceptions;
using TaskManagement.Services;
using Xunit;

namespace TaskManagement.Tests;

public class AuthServiceTests
{
    private static IConfiguration Config() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-secret-key-test-secret-key-test-secret-key-1234567890",
            ["Jwt:Issuer"] = "TaskManagement.Tests",
            ["Jwt:Audience"] = "TaskManagement.Tests",
            ["Jwt:ExpiryMinutes"] = "60"
        })
        .Build();

    private static AuthService CreateService()
        => new(TestDb.Create(), Config());

    [Fact]
    public async Task Register_ValidRequest_ReturnsTokenAndUserWithRoleUser()
    {
        var service = CreateService();

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Name = "New User",
            Email = "new@example.com",
            Password = "New@123"
        });

        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal("new@example.com", result.User.Email);
        Assert.Equal("User", result.User.Role); // self-registration is never admin
    }

    [Fact]
    public async Task Register_EmptyFields_Throws400()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.RegisterAsync(
            new RegisterRequest { Name = "", Email = "", Password = "" }));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Throws409()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.RegisterAsync(
            new RegisterRequest { Name = "X", Email = "ADMIN@taskmgmt.com", Password = "X@123" }));

        Assert.Equal(409, ex.StatusCode); // case-insensitive email check
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var service = CreateService();

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@taskmgmt.com",
            Password = "Admin@123"
        });

        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal("Admin", result.User.Role);
    }

    [Fact]
    public async Task Login_WrongPassword_Throws401()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.LoginAsync(
            new LoginRequest { Email = "admin@taskmgmt.com", Password = "Wrong!" }));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Throws401_WithSameMessage()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.LoginAsync(
            new LoginRequest { Email = "nobody@example.com", Password = "Anything1" }));

        // Same message as a wrong password — don't reveal which emails exist.
        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("Invalid email or password.", ex.Message);
    }
}
