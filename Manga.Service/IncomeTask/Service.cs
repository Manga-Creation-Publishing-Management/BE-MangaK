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

    public Task<Response.GetIncomeResponse> GetIncome(Request.GetIncomeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response.GetIncomeHistoryResponse> GetIncomeHistory(Request.GetIncomeHistoryRequest request)
    {
        throw new NotImplementedException();
    }
}