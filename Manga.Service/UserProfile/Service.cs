using Manga.Repository.Data;
using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.UserProfile;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MediaService.IService _mediaService;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, MediaService.IService mediaService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _mediaService = mediaService;
    }

    public async Task<Response.GetProfileResponse> GetProfile()
    {
var user =  await GetUserIdCurrent();
        return new Response.GetProfileResponse()
        {
            Id =  user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            AuthorName = user.AuthorName,
        };
    }

    public async Task<Response.GetProfileResponse> UpdateProfile(Request.UpdateProfileRequest request)
    {
        
        var user = await GetUserIdCurrent();
        user.FirstName = !string.IsNullOrWhiteSpace(request.FirstName) ? request.FirstName : user.FirstName;
        user.LastName = !string.IsNullOrWhiteSpace(request.LastName) ? request.LastName : user.LastName;
        user.AuthorName = !string.IsNullOrWhiteSpace(request.AuthorName) ? request.AuthorName : user.AuthorName;
        user.Phone = !string.IsNullOrWhiteSpace(request.Phone) ? request.Phone : user.Phone;

        user.Bio = !string.IsNullOrWhiteSpace(request.Bio) ? request.Bio : user.Bio;
        if (request.AvatarFile != null && request.AvatarFile.Length > 0)
        {
           user.AvatarUrl = await _mediaService.UploadImageAsync(request.AvatarFile);
        }
        
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return new Response.GetProfileResponse()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            AuthorName = user.AuthorName,
            Phone = user.Phone,
        };
    }

    private async Task<User> GetUserIdCurrent()
    {
        var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;
        if(userId == null) throw new UnauthorizedAccessException("You must log in");
        var userIdGuild = Guid.Parse(userId);
        var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.Id == userIdGuild);
        if(user == null || user.IsDeleted) throw new UnauthorizedAccessException("Account disabled");
        return user;
    }
    public async Task<List<Response.GetUserListByRole>> GetUserListByRole(Request.GetUserListByRoleRequest request)
    {
        var usersList = await _dbContext.Users.Where(c => !c.IsDeleted && c.Role == request.UserRole)
            .OrderBy(c => c.FirstName)
            .Select(c => new Response.GetUserListByRole()
            {
                UserId = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName
            })
            . ToListAsync();
        return usersList;
    }

 
}