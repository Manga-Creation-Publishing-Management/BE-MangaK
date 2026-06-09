using Manga.Repository.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.Category;

public class Service:IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Response.GetCategoryResponse>> GetCategories()
    {
        var categoryList = await _dbContext.Categories.Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name).ToListAsync();

        return categoryList.Select(c => new Response.GetCategoryResponse()
        {
            CategoryId = c.Id,
            Name = c.Name
        }).ToList();
    }
}