using Manga.Service.Feedback;
using Manga.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IService _feedbackService;

    public FeedbackController(IService feedbackService)
    {
        _feedbackService = feedbackService;
    }
    [Authorize]
    [HttpPost("send-feedback")]
    public async Task<IActionResult> SendFeedback([FromBody] Request.SendFeedbackRequest request)
    {
        var result = await _feedbackService.SendFeedback(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Send feedback Successfully", HttpContext.TraceIdentifier));
    }
}