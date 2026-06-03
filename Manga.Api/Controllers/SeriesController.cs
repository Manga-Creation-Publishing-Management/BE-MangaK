using Manga.Service.Model;
using Manga.Service.Series;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeriesController: ControllerBase
{
   private readonly IService _seriesService;

   public SeriesController(IService seriesService)
   {
      _seriesService = seriesService;
   }
   
   [Authorize]
   [HttpPost("create-series")]
   public async Task<IActionResult> CreateSeries([FromForm] Request.CreateSeriesRequest request)
   {
         var createSeries = await _seriesService.CreateSeries(request);
         return Ok(ApiResponseFactory.SuccessResponse(createSeries, "Create Cart Successfully", HttpContext.TraceIdentifier));
   }
   
   [HttpGet("get-all-series")]
   public async Task<IActionResult> GetAllSeries()
   {
       var result = await _seriesService.GetAllSeries();
       return Ok(ApiResponseFactory.SuccessResponse(result, "Get All Series Successfully", HttpContext.TraceIdentifier));
   }
   
   [HttpGet("get-series-details")]
   public async Task<IActionResult> GetSeriesDetails(Guid seriesId)
   {
       var result = await _seriesService.GetSeriesDetails(seriesId);
       return Ok(ApiResponseFactory.SuccessResponse(result, "Get Series Detail Successfully", HttpContext.TraceIdentifier));
   }
   
   [HttpGet("get-series-by-title")]
   public async Task<IActionResult> GetSeriesByTitle([FromQuery] string title)
   {
       var result = await _seriesService.GetSeriesByTitle(title);
       return Ok(ApiResponseFactory.SuccessResponse(result, "Get series by title successfully.", HttpContext.TraceIdentifier));
   }
  
   [Authorize]
   [HttpPatch("tantou-review/{seriesId}")]
   public async Task<IActionResult> ReviewSeriesByTantouEditor(Guid seriesId,[FromBody] Request.ReviewSeriesRequest request)
   {
       var result = await _seriesService.ReviewSeriesByTantouEditor(seriesId, request);
       return Ok(ApiResponseFactory.SuccessResponse(result, "Series reviewed by TantouEditor.", HttpContext.TraceIdentifier));
   }
   
   [Authorize]
   [HttpPatch("board-review/{seriesId}")]
   public async Task<IActionResult> ApprovedSeriesByEditorialBoard(Guid seriesId, [FromBody] Request.ReviewSeriesRequest request)
   {
       var result = await _seriesService.ApprovedSeriesByEditorialBoard(seriesId, request);
       return Ok(ApiResponseFactory.SuccessResponse(result, "Series reviewed by EditorialBoard.", HttpContext.TraceIdentifier));
   }
}