using Manga.Service.Category;
using Manga.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController: ControllerBase
{
    private readonly IService _categoryService;

    public CategoryController(IService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet("get-category")]
    public async Task<IActionResult> GetCategory()
    {
        var result = await _categoryService.GetCategories();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Category Successfully", HttpContext.TraceIdentifier));
    }
}