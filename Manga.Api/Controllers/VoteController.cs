using Manga.Api.extensions;
using Manga.Service.ChapterVoting;
using Manga.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoteController : ControllerBase
{
    private readonly IService _chapterVotingService;

    public VoteController(IService chapterVotingService)
    {
        _chapterVotingService = chapterVotingService;
    }

    [Authorize(Policy = JwtExtensions.ReaderPolicy)]
    [HttpPost("voting-chapter")]
    public async Task<IActionResult> VoteChapter([FromBody] Request.VoteChapterRequest request)
    {
        var result = await _chapterVotingService.VoteChapter(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Vote Chapter Successfully", HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpGet("calculate-chapter-vote")]
    public async Task<IActionResult> CalculateChapterVote()
    {
        var result = await _chapterVotingService.CalculateChapterVote();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Calculate Chapter Vote Successfully",
            HttpContext.TraceIdentifier));
    }
    
    [Authorize(Policy = JwtExtensions.ReaderPolicy)]
    [HttpGet("{chapterId}")]
    public async Task<IActionResult> GetReaderVote(Guid chapterId)
    {
        var result = await _chapterVotingService.GetReaderVote(chapterId);

        return Ok(ApiResponseFactory.SuccessResponse(result, "Get reader vote successfully", HttpContext.TraceIdentifier));
    }
}