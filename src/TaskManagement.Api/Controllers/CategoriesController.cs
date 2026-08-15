using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTOs;
using TaskManagement.Services;

namespace TaskManagement.Api.Controllers;

/// <summary>
/// Category endpoints. Read-only list for the category dropdown and badges.
/// </summary>
[ApiController]
[Authorize]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>GET /api/categories — all categories, in seed order.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> GetAll()
        => Ok(await _categoryService.GetCategoriesAsync());
}
