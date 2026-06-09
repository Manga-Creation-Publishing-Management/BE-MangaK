using Manga.Service.Chapter;
using Manga.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChapterController: ControllerBase
{
    private readonly IService _chapterService;

    public ChapterController(IService chapterService)
    {
        _chapterService = chapterService;
    }
    
    [HttpPost("create-chapter")]
    public async Task<IActionResult> CreateChapter(Guid seriesId, [FromForm] Request.CreateChapterRequest request)
    {
        var createSeries = await _chapterService.CreateChapter(seriesId, request);
        return Ok(ApiResponseFactory.SuccessResponse(createSeries, "Create Chapter Successfully", HttpContext.TraceIdentifier));
    }

    [HttpGet("get-all-chapters")]
    public async Task<IActionResult> GetAllChapter(Guid seriesId)
    {
        var result = await _chapterService.GetAllChapters(seriesId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get All Chapter Successfully", HttpContext.TraceIdentifier));
    }
 
    [HttpGet("get-chapter-details")]
    public async Task<IActionResult> GetChapterDetail(Guid seriesId, Guid chapterId)
    {
        var result = await _chapterService.GetChapterDetails(seriesId, chapterId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Chapter Chapter Successfully", HttpContext.TraceIdentifier));
    }
}