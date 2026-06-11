using Manga.Service.IncomeTask;
using Manga.Service.Model;
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

    [HttpGet("get-income-tasks")]
    public async Task<IActionResult> GetIncomeTasks([FromQuery] Request.GetIncomeRequest request)
    {
        var (incomes, totalAmount) = await _incomeTaskService.GetIncome(request);
    
        // Gộp chung vào một anonymous object để JSON sinh ra đúng định dạng đẹp đẽ
        var responseData = new 
        {
            Incomes = incomes,
            TotalMonth = totalAmount
        };
        // var result =  await _incomeTaskService.GetIncome(request);
        return Ok(ApiResponseFactory.SuccessResponse(responseData, "Get income task successfully",
            HttpContext.TraceIdentifier));
    }
    [HttpGet("get-income-tasks-history")]
    public async Task<IActionResult> GetIncomeTasksHistory()
    {
        var result =  await _incomeTaskService.GetIncomeHistory();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get income task history successfully",
            HttpContext.TraceIdentifier));
    }
}