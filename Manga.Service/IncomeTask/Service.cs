using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.IncomeTask;

public class Service : IService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;

    public Service(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task<(List<Response.GetIncomeResponse>, decimal IncomeMonth)> GetIncome(Request.GetIncomeRequest request)
    {
        var userIdGuild = GetCurrentUserId();
        
        var now = DateTimeOffset.UtcNow;
        int currentMonth = (request.month.HasValue && request.month > 0) ? request.month.Value : now.Month;
        int currentYear = (request.year.HasValue && request.year > 0) ? request.year.Value : now.Year;
    
        var incomes = await _dbContext.Incomes
            .Where(i => (i.MangaTask.AssignedToId == userIdGuild || i.MangaTask.CreatedById == userIdGuild)
                        && i.CreatedAt.Year == currentYear   
                        && i.CreatedAt.Month == currentMonth
           ).Select(i => new Response.GetIncomeResponse
            {
                IncomeId = i.Id,
                Amount = i.Amount,
                PaidAt = i.Date.HasValue ? i.Date.Value : DateTime.Today,
                Status = i.Status,
                TaskTitle = i.MangaTask != null ? i.MangaTask.TaskTitle : "Don't connect Task",
            })
            .ToListAsync();
        
        decimal totalAmount = incomes.Sum(x => x.Amount);
        return (incomes , totalAmount);
    }

    public async Task<List<Response.GetIncomeHistoryResponse>> GetIncomeHistory()
    {
        var userIdGuild = GetCurrentUserId();
        
        var now = DateTimeOffset.UtcNow;
        var historyIncome = await _dbContext.Incomes.Where (x => x.MangaTask.AssignedToId == userIdGuild
        && (x.CreatedAt.Year < now.Year || (x.CreatedAt.Year == now.Year && x.CreatedAt.Month < now.Month)))
            .GroupBy(x => new
            {
                Year = x.CreatedAt.Year,
                Month = x.CreatedAt.Month
            })
            .Select(x => new  Response.GetIncomeHistoryResponse
            {
               Month =  x.Key.Month,
               Year =  x.Key.Year,
               TotalIncome = x.Sum(y => y.Amount)
            })
            .OrderByDescending(res => res.Year)
            .ThenByDescending(res => res.Month)
            .ToListAsync();
        return historyIncome;
    }
    private Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;
            
        if (string.IsNullOrEmpty(userId)) 
            throw new UnauthorizedAccessException("You must log in");

        return Guid.Parse(userId);
    }
}