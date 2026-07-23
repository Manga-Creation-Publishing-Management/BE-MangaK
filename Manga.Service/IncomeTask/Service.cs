
﻿using System.ComponentModel.DataAnnotations;
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
        var userIdGuid = GetCurrentUserId();
        
        var now = DateTimeOffset.UtcNow;
        int currentMonth = (request.month.HasValue && request.month > 0) ? request.month.Value : now.Month;
        int currentYear = (request.year.HasValue && request.year > 0) ? request.year.Value : now.Year;
        var startOfMonth = new DateTimeOffset(currentYear, currentMonth, 1, 0, 0, 0, TimeSpan.Zero);
        var endOfMonth = startOfMonth.AddMonths(1);

        var userRole = await _dbContext.Users
            .Where(u => u.Id == userIdGuid && !u.IsDeleted && u.Status == UserStatus.Active)
            .Select(u => u.Role)
            .FirstOrDefaultAsync();

        var query = _dbContext.Incomes.AsQueryable();
        if (userRole == UserRole.Assistant)
        {
            query = query.Where(i => i.MangaTask.AssignedToId == userIdGuid);
        }
        else if (userRole == UserRole.Mangaka)
        {
            query = query.Where(i => i.MangaTask.CreatedById == userIdGuid);
        }
        else
        {
            return (new List<Response.GetIncomeResponse>(), 0);
        }
        query = query.Where(i => i.CreatedAt >= startOfMonth && i.CreatedAt < endOfMonth);

        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        var incomes = await query.Select(i => new Response.GetIncomeResponse
            {
                IncomeId = i.Id,
                Amount = userRole == UserRole.Mangaka ? -i.Amount : i.Amount,
                PaidAt = i.Date, 
                Status = i.Status,
                TaskTitle = i.MangaTask != null ? i.MangaTask.TaskTitle : "Don't connect Task",
            })
            .ToListAsync();
        
        decimal totalAmount = incomes.Sum(x => x.Amount);
        return (incomes, totalAmount);
    }

    public async Task<List<Response.GetIncomeHistoryResponse>> GetIncomeHistory()
    {
        var userIdGuild = GetCurrentUserId();
        
        var now = DateTimeOffset.UtcNow;
        var startOfCurrentMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var userRole = await _dbContext.Users
            .Where(u => u.Id == userIdGuild && !u.IsDeleted && u.Status == UserStatus.Active)
            .Select(u => u.Role)
            .FirstOrDefaultAsync();

        var query = _dbContext.Incomes.AsQueryable();

        if (userRole == UserRole.Assistant)
        {
            query = query.Where(i => i.MangaTask.AssignedToId == userIdGuild);
        }
        else if (userRole == UserRole.Mangaka)
        {
            query = query.Where(i => i.MangaTask.CreatedById == userIdGuild);
        }
        else
        {
            return new List<Response.GetIncomeHistoryResponse>();
        }
        query = query.Where(x => x.CreatedAt < startOfCurrentMonth);
        
        var historyIncome = await query.Where (x => x.MangaTask.AssignedToId == userIdGuild
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
               TotalIncome = userRole == UserRole.Mangaka ? -x.Sum(y => y.Amount) : x.Sum(y => y.Amount)
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