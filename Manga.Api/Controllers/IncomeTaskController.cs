using Manga.Api.extensions;
using Manga.Service.IncomeTask;
using Manga.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class IncomeTaskController : ControllerBase
{ 
    private readonly IService _incomeTaskService;

    public IncomeTaskController(IService incomeTaskService)
    {
        _incomeTaskService = incomeTaskService;
    }
    [Authorize]
    [HttpGet("get-income-tasks")]
    public async Task<IActionResult> GetIncomeTasks([FromQuery] Request.GetIncomeRequest request)
    {
        var (incomes, totalAmount) = await _incomeTaskService.GetIncome(request);
    
        var responseData = new 
        {
            Incomes = incomes,
            TotalMonth = totalAmount
        };
        return Ok(ApiResponseFactory.SuccessResponse(responseData, "Get income task successfully",
            HttpContext.TraceIdentifier));
    }
    [Authorize]
    [HttpGet("get-income-tasks-history")]
    public async Task<IActionResult> GetIncomeTasksHistory()
    {
        var result =  await _incomeTaskService.GetIncomeHistory();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get income task history successfully",
            HttpContext.TraceIdentifier));
    }
}