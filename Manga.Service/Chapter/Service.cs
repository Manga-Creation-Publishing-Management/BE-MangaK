using Manga.Repository.Data;
using Microsoft.AspNetCore.Http;

namespace Manga.Service.Chapter;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public Task<Response.CreateChapterResponse> CreateChapter(Guid seriesId, Request.CreateChapterRequest request)
    {
        throw new NotImplementedException();
    }
}