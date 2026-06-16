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
        var userId = GetUserIdCurrent();
        
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(user => new Response.GetProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                AuthorName = user.AuthorName,
                Phone = user.Phone,
                SupervisorId = user.SupervisorId
            })
            .FirstOrDefaultAsync();

        if (user == null) 
            throw new UnauthorizedAccessException("Account disabled or does not exist");
        return user;
    }

    public async Task<Response.GetProfileResponse> UpdateProfile(Request.UpdateProfileRequest request)
    {
        
        var userId = GetUserIdCurrent();
        var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.Id == userId && !c.IsDeleted);
        
        if (user == null) 
            throw new UnauthorizedAccessException("Account disabled or does not exist");
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
            SupervisorId = user.SupervisorId
        };
    }

    public async Task<List<Response.GetUserListResponse>> GetUserList()
    {
        var usersList = await _dbContext.Users.Where(c => !c.IsDeleted )
            .OrderBy(c => c.FirstName)
            .Select(c => new Response.GetUserListResponse()
            {
                UserId = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone ?? "",
                AvatarUrl = c.AvatarUrl ?? "",
                Bio = c.Bio ?? "",
                AuthorName = c.AuthorName ?? "",
                Role = c.Role,
                Status =  c.Status,
                SupervisorId = c.SupervisorId
            })
            .AsNoTracking()
            . ToListAsync();
        return usersList;
    }
    public async Task<List<Response.GetUserListByRoleResponse>> GetUserListByRole(Request.GetUserListByRoleRequest request)
    {
        var usersList  = await _dbContext.Users.Where(c => !c.IsDeleted && c.Role == request.UserRole )
            .OrderBy(c => c.FirstName)
            .Select(c => new Response.GetUserListByRoleResponse()
            {
                UserId = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
               })
            .AsNoTracking()
            . ToListAsync();
        return usersList;
    }

    public async Task<Response.GetProfileResponse> UpdateUserStatus(Request.UpdateUserStatusRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.Id == request.UserId);
        if (user == null || user.IsDeleted) throw new InvalidOperationException("This account not found or was deleted.");
        user.Status = request.Status;
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
            SupervisorId = user.SupervisorId
        };
    }

    public async Task<List<Response.GetUserListByRoleResponse>> FilterAssistant(Request.FilterAssistantRequest request)
    {
        var userId =  GetUserIdCurrent();
        var userCheck =  await _dbContext.Users.AnyAsync(c => c.Id == userId && !c.IsDeleted && c.Role == UserRole.Mangaka);
        if (!userCheck) throw new InvalidOperationException("This account not found or was deleted. Or You aren't mangaka");

        var listAssistant = await _dbContext.Users.Where(c => !c.IsDeleted && c.Role == UserRole.Assistant
                                                                           && !_dbContext.MangaTasks.Any(x => x.AssignedToId == c.Id &&
                                                                               x.ChapterId == request.ChapterId))
            .Select(u => new Response.GetUserListByRoleResponse()
            {
                UserId = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
            }).ToListAsync();
        return listAssistant;
    }

    public async Task<List<Response.GetUserListResponse>> GetReaderList()
    {
        var readers = await _dbContext.Readers
            .OrderBy(c => c.Name)
            .Select(c => new Response.GetUserListResponse()
            {
                UserId = c.Id,
                Email = c.Email,
                FirstName = c.Name ?? "",
                LastName = "",
                Phone = "",
                AvatarUrl = c.AvatarUrl ?? "",
                Bio = "",
                AuthorName = "",
                Role = UserRole.Reader,
                Status =  c.Status,
                SupervisorId = null
            })
            .AsNoTracking()
            .ToListAsync();
        return readers;
    }

    public async Task<List<Response.GetUserListResponse>> FilterTantouList()
    {
        var tantous = await _dbContext.Users
            .Where(c => !c.IsDeleted && c.Role == UserRole.Tantou && c.Mangakas.Count(m => !m.IsDeleted && m.Role == UserRole.Mangaka) < 3)
            .OrderBy(c => c.FirstName)
            .Select(c => new Response.GetUserListResponse()
            {
                UserId = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone ?? "",
                AvatarUrl = c.AvatarUrl ?? "",
                Bio = c.Bio ?? "",
                AuthorName = c.AuthorName ?? "",
                Role = c.Role,
                Status =  c.Status,
                SupervisorId = c.SupervisorId
            })
            .AsNoTracking()
            .ToListAsync();
        return tantous;
    }

    private Guid GetUserIdCurrent()
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userIdGuid))
        {
            throw new UnauthorizedAccessException("You must log in");
        }

        return userIdGuid;
    }
}