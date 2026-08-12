using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TaskManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Services -------------------------------------------------------------
builder.Services.AddControllers();

// Entity Framework Core against SQL Server (LocalDB/Express via connection string)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "Web API for the Task Management Tool (cohort 9 assignment)."
    });

    // JWT bearer auth is added in the auth phase (feature/auth-api).
});

// CORS: allow the Vite dev server (http://localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// --- Pipeline -------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

// --- Startup: apply pending migrations + seed default data (dev convenience) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbInitializer.SeedAsync(db);
}

app.Run();

// Exposed for integration tests
public partial class Program { }
