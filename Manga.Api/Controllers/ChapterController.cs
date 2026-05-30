using Manga.Service.Chapter;
using Manga.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ChapterController: ControllerBase
{
    private readonly IService _chapterService;

    public ChapterController(IService chapterService)
    {
        _chapterService = chapterService;
    }
    
    [HttpPost("create-chapter")]
    public async Task<IActionResult> CreateChapter(Guid seriesId, [FromBody] Request.CreateChapterRequest request)
    {
        var createSeries = await _chapterService.CreateChapter(seriesId, request);
        return Ok(ApiResponseFactory.SuccessResponse(createSeries, "Create Chapter Successfully", HttpContext.TraceIdentifier));
    }
}