using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.Feedback;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> SendFeedback(Request.SendFeedbackRequest request)
    {
        var userId = GetUserIdCurrent();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) throw new UnauthorizedAccessException("Unauthorized");
        
        if(user.Role == UserRole.Assistant) throw new UnauthorizedAccessException("Assistant can't send feedback");
        
        var checkSeries = await _dbContext.Series.AnyAsync(x => x.Id == request.SeriesId && x.IsDeleted == false);
        if (!checkSeries) throw new KeyNotFoundException("Series not found or deleted");

        var feedback = new Repository.Entity.Feedback()
        {
            Id = Guid.NewGuid(),
            SenderId = user.Id,
            Content = request.Content,
            SeriesId = request.SeriesId,
            ChapterId = request.ChapterId,
            MangaTaskId = request.MangaId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Feedbacks.Add(feedback);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Response.GetFeedBackDetailResponse>> GetFeedBackDetail(Request.GetFeedBackRequest request)
    {
        var userId = GetUserIdCurrent();
        var  user = await _dbContext.Users.AnyAsync(x => x.Id == userId);
        if (!user) throw new UnauthorizedAccessException("Unauthorized");
        
        IQueryable<Repository.Entity.Feedback> query = _dbContext.Feedbacks.Where(x => x.SeriesId == request.SeriesId && x.IsDeleted == false).AsNoTracking();
        if (request.ChapterId.HasValue)
        {
            query = query.Where(x => x.ChapterId == request.ChapterId.Value);
        }

        if (request.MangaTaskId.HasValue)
        {
            query = query.Where(x => x.MangaTaskId == request.MangaTaskId.Value);
        }

        var feedback = await query.Select( f => 
            new Response.GetFeedBackDetailResponse()
            {
                Id = f.Id,
                SenderId = f.SenderId,
                SenderName = ((f.Sender.FirstName ?? "") + " " + (f.Sender.LastName ?? "")).Trim(),
                SeriesTitle =  f.Series != null ? f.Series.Title : null,
                ChapterTitle = f.Chapter != null ? f.Chapter.Title : null,
                MangaTaskTitle = f.MangaTask != null ? f.MangaTask.TaskTitle : null,
                Content = f.Content,
                CreatedAt = f.CreatedAt
            }
        ).ToListAsync();
        return feedback;
    }

    public async Task<List<Response.GetFeedBackDetailResponse>> GetFeedbackList()
    {
        var userId = GetUserIdCurrent();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null || user.IsDeleted) throw new UnauthorizedAccessException("Unauthorized");

        IQueryable<Repository.Entity.Feedback> query = _dbContext.Feedbacks
            .Include(f => f.Sender)
            .Include(f => f.Series)
            .Include(f => f.Chapter)
            .Include(f => f.MangaTask)
            .Where(f => !f.IsDeleted).AsNoTracking();

        if (user.Role == UserRole.Mangaka)
        {
            query = query.Where(f => 
                (f.Series != null && f.Series.CreatedById == user.Id) || 
                (f.Chapter != null && f.Chapter.Series.CreatedById == user.Id) ||
                (f.MangaTask != null && f.MangaTask.Chapter.Series.CreatedById == user.Id) 
            );
            query = query.Where(f => f.Sender.Role == UserRole.Tantou || f.Sender.Role == UserRole.Editorial);
        }
        else if (user.Role == UserRole.Tantou)
        {
            query = query.Where(f => 
                (f.Series != null && f.Series.CreatedBy.SupervisorId == user.Id) || 
                (f.Chapter != null && f.Chapter.Series.CreatedBy.SupervisorId == user.Id) ||
                (f.MangaTask != null && f.MangaTask.Chapter.Series.CreatedBy.SupervisorId == user.Id) 
            );
            query = query.Where(f => f.Sender.Role == UserRole.Editorial || f.SenderId == user.Id);
        }
        else if (user.Role == UserRole.Editorial)
        {
            query = query.Where(f => f.SenderId == user.Id || 
                                     (f.Series != null && f.Series.ApprovedById == user.Id) ||
                                     (f.Chapter != null && f.Chapter.Series.ApprovedById == user.Id) ||
                                     (f.MangaTask != null && f.MangaTask.Chapter.Series.ApprovedById == user.Id));
        }
        else 
        {
            query = query.Where(f => f.SenderId == user.Id);
        }

        var feedback = await query.OrderByDescending(f => f.CreatedAt).Select( f => 
            new Response.GetFeedBackDetailResponse()
            {
                Id = f.Id,
                SenderId = f.SenderId,
                SenderName = ((f.Sender.FirstName ?? "") + " " + (f.Sender.LastName ?? "")).Trim(),
                SeriesTitle =  f.Series != null ? f.Series.Title : null,
                ChapterTitle = f.Chapter != null ? f.Chapter.Title : null,
                MangaTaskTitle = f.MangaTask != null ? f.MangaTask.TaskTitle : null,
                Content = f.Content,
                CreatedAt = f.CreatedAt
            }
        ).ToListAsync();
        
        return feedback;
    }

    private Guid GetUserIdCurrent()
    {
        var userId = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;
            
        if (string.IsNullOrEmpty(userId)) 
            throw new UnauthorizedAccessException("You must log in");

        return Guid.Parse(userId);
    }
}