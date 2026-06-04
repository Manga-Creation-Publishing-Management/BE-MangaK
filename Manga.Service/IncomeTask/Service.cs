using System.ComponentModel.DataAnnotations;
using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.IncomeTask;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    // public async Task<Response.CreateIncomeResponse> CreateIncome(Request.CreateIncomeRequest request)
    // {
    //     /*
    //     1. Check người đang hd có phải là Mangaka?
    //     2. Check trạng thái của Task ?
    //      */
    //     var userId = _httpContextAccessor.HttpContext.User!.Claims.FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;
    //     if(userId == null) throw new UnauthorizedAccessException("You must log in");
    //     var userIdGuild = Guid.Parse(userId);
    //     var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuild);
    //     if(user == null) throw new UnauthorizedAccessException("User not found");
    //     if(user.Role != UserRole.Mangaka) throw new UnauthorizedAccessException("You do not have permission to do this action");
    //     
    //     var mangaTask = await _dbContext.MangaTasks.FirstOrDefaultAsync(x => x.Id == request.MangaTaskId);
    //     if(mangaTask == null) throw new KeyNotFoundException("Manga task does not exist");
    //     if(mangaTask.IsDeleted || mangaTask.Status == MangaTaskStatus.Complete) throw new ValidationException("Manga task is complete or Manga task is already in progress");
    //
    //     var incomeTask = new Repository.Entity.Income()
    //     {
    //         Id = Guid.NewGuid(),
    //         Amount =  request.Amount,
    //         MangaTaskId = request.MangaTaskId,
    //         MangaTask =  mangaTask,
    //         CreatedAt =  DateTimeOffset.Now,
    //     };
    //     _dbContext.Add(incomeTask);
    //     await _dbContext.SaveChangesAsync();
    //     return new Response.CreateIncomeResponse()
    //     {
    //         IncomeId = incomeTask.Id,
    //         Amount = incomeTask.Amount,
    //         Status = incomeTask.Status,
    //         MangaTaskId = incomeTask.MangaTaskId,
    //         TaskTitle = mangaTask.TaskTitle
    //     };
    // }

    public Task<Response.GetIncomeResponse> GetIncome(Request.GetIncomeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response.GetIncomeHistoryResponse> GetIncomeHistory(Request.GetIncomeHistoryRequest request)
    {
        throw new NotImplementedException();
    }
}