using Manga.Api.extensions;
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
   
   [Authorize(Policy = JwtExtensions.MangakaPolicy)]
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
}