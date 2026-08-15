using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTOs;
using TaskManagement.Services;

namespace TaskManagement.Api.Controllers;

/// <summary>
/// Task endpoints. Every route needs a valid Bearer token ([Authorize]).
/// The viewer's id and role come from the validated JWT claims — the same
/// source the frontend uses to decide what to show.
/// </summary>
[ApiController]
[Authorize]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>GET /api/tasks/dashboard/stats — the dashboard counters.</summary>
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<DashboardStatsResponse>> GetStats()
        => Ok(await _taskService.GetDashboardStatsAsync(ViewerId, IsAdmin));

    /// <summary>GET /api/tasks?status=&amp;priority=&amp;search=... — filtered list.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TaskResponse>>> GetAll([FromQuery] TaskQuery query)
        => Ok(await _taskService.GetTasksAsync(query, ViewerId, IsAdmin));

    /// <summary>GET /api/tasks/{id} — a single task.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Get(int id)
        => Ok(await _taskService.GetTaskAsync(id, ViewerId, IsAdmin));

    /// <summary>POST /api/tasks — create a task.</summary>
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request)
        => Ok(await _taskService.CreateTaskAsync(request, ViewerId));

    /// <summary>PUT /api/tasks/{id} — update a task.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, [FromBody] UpdateTaskRequest request)
        => Ok(await _taskService.UpdateTaskAsync(id, request, ViewerId, IsAdmin));

    /// <summary>DELETE /api/tasks/{id} — delete a task.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteTaskAsync(id, ViewerId, IsAdmin);
        return NoContent(); // 204 — the frontend treats this as success
    }

    // --- who is the caller? (from the validated JWT) ----------------------------
    private int ViewerId
        => int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : 0;

    private bool IsAdmin => User.IsInRole("Admin");
}
