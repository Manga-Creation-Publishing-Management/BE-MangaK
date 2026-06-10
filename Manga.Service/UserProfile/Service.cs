using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.UserProfile;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Response.GetProfileResponse> GetProfile()
    {
        throw new NotImplementedException();
    }

    public async Task<List<Response.GetUserListByRole>> GetUserListByRole(Request.GetUserListByRoleRequest request)
    {
        var usersList = await _dbContext.Users.Where(c => !c.IsDeleted && c.Role == request.UserRole)
            .OrderBy(c => c.FirstName)
            .ToListAsync();
        return usersList.Select(c => new Response.GetUserListByRole()
        {
            UserId = c.Id,
            Email = c.Email,
            FirstName = c.FirstName,
            LastName = c.LastName
        }).ToList();
    }
}