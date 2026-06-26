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
        return Ok(ApiResponseFactory.SuccessResponse(result, "Send feedback Successfully",
            HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpGet("get-feedback-list")]
    public async Task<IActionResult> GetFeedbackList()
    {
        var result = await _feedbackService.GetFeedbackList();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get feedback list Successfully",
            HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpGet("get-feedback-detail")]
    public async Task<IActionResult> GetFeedbackDetail([FromQuery] Request.GetFeedBackRequest request)
    {
        var result = await _feedbackService.GetFeedBackDetail(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get feedback detail Successfully",
            HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpPatch("mark-as-read/{feedbackId}")]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid feedbackId)
    {
        var result = await _feedbackService.MarkAsRead(feedbackId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Mark feedback as read successfully",
            HttpContext.TraceIdentifier));
    }
}