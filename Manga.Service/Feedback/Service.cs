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
        
        if (user.Role == UserRole.Assistant) throw new UnauthorizedAccessException("Assistant can't send feedback");

        Repository.Entity.Series? series = null;
        Guid? seriesId = request.SeriesId;
        Guid? chapterId = request.ChapterId;

        if (request.MangaTaskId.HasValue)
        {
            var task = await _dbContext.MangaTasks
                .Include(t => t.Chapter)
                .ThenInclude(c => c.Series)
                .ThenInclude(s => s.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == request.MangaTaskId.Value && !t.IsDeleted);
            if (task == null) throw new KeyNotFoundException("MangaTask not found");
            
            series = task.Chapter?.Series;
            seriesId = series?.Id;
            chapterId = task.ChapterId;
        }
        else if (request.ChapterId.HasValue)
        {
            var chapter = await _dbContext.Chapters
                .Include(c => c.Series)
                .ThenInclude(s => s.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == request.ChapterId.Value && !c.IsDeleted);
            if (chapter == null) throw new KeyNotFoundException("Chapter not found");
            
            series = chapter.Series;
            seriesId = series?.Id;
        }
        else if (request.SeriesId.HasValue)
        {
            series = await _dbContext.Series
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(x => x.Id == request.SeriesId.Value && !x.IsDeleted);
        }

        if (series == null) throw new KeyNotFoundException("Series not found or deleted");

        // Role-based validation
        if (user.Role == UserRole.Mangaka)
        {
            if (!request.MangaTaskId.HasValue)
                throw new InvalidOperationException("Mangaka can only send feedback for tasks");

            if (series.CreatedById != user.Id)
                throw new UnauthorizedAccessException("You can only send feedback for your own series");
        }
        else if (user.Role == UserRole.Tantou)
        {
            if (request.MangaTaskId.HasValue)
                throw new InvalidOperationException("Tantou cannot send feedback for tasks");

            if (series.ReviewedById != user.Id && (series.CreatedBy == null || series.CreatedBy.SupervisorId != user.Id))
                throw new UnauthorizedAccessException("You can only send feedback for series you review or supervise");
        }
        else if (user.Role != UserRole.Editorial && user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("You don't have permission to send feedback");
        }

        var feedback = new Repository.Entity.Feedback()
        {
            Id = Guid.NewGuid(),
            SenderId = user.Id,
            Content = request.Content,
            SeriesId = seriesId,
            ChapterId = chapterId,
            MangaTaskId = request.MangaTaskId,
            Type = request.Type,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Feedbacks.Add(feedback);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Response.GetFeedBackDetailResponse>> GetFeedBackDetail(Request.GetFeedBackRequest request)
    {
        var userId = GetUserIdCurrent();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null || user.IsDeleted) throw new UnauthorizedAccessException("Unauthorized");

        var series = await _dbContext.Series
            .Include(s => s.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == request.SeriesId && x.IsDeleted == false);
        if (series == null) throw new KeyNotFoundException("Series not found");

        // Row level security: check if user has access to view this series' feedbacks
        if (user.Role == UserRole.Mangaka)
        {
            if (series.CreatedById != user.Id)
                throw new UnauthorizedAccessException("You are not the creator of this series");
        }
        else if (user.Role == UserRole.Tantou)
        {
            if (series.ReviewedById != user.Id && (series.CreatedBy == null || series.CreatedBy.SupervisorId != user.Id))
                throw new UnauthorizedAccessException("You do not supervise or review this series");
        }
        else if (user.Role == UserRole.Assistant)
        {
            if (!request.MangaTaskId.HasValue)
            {
                throw new UnauthorizedAccessException("Assistant can only query feedback for specific tasks");
            }
            var task = await _dbContext.MangaTasks.FirstOrDefaultAsync(t => t.Id == request.MangaTaskId.Value);
            if (task == null || task.AssignedToId != user.Id)
            {
                throw new UnauthorizedAccessException("You are not assigned to this task");
            }
        }
        else if (user.Role != UserRole.Editorial && user.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("You don't have permission to view feedbacks");
        }
        
        IQueryable<Repository.Entity.Feedback> query = _dbContext.Feedbacks.Where(x => x.SeriesId == request.SeriesId && x.IsDeleted == false).AsNoTracking();
        if (request.ChapterId.HasValue)
        {
            query = query.Where(x => x.ChapterId == request.ChapterId.Value);
        }

        if (request.MangaTaskId.HasValue)
        {
            query = query.Where(x => x.MangaTaskId == request.MangaTaskId.Value);
        }

        // Apply visibility rules for different roles
        if (user.Role == UserRole.Assistant)
        {
            query = query.Where(f => f.MangaTask != null && f.MangaTask.AssignedToId == user.Id && f.Sender.Role == UserRole.Mangaka);
        }
        else if (user.Role == UserRole.Mangaka)
        {
            query = query.Where(f => f.Sender.Role == UserRole.Tantou || f.Sender.Role == UserRole.Editorial || f.SenderId == user.Id);
        }
        else if (user.Role == UserRole.Tantou)
        {
            query = query.Where(f => f.Sender.Role == UserRole.Editorial || f.Sender.Role == UserRole.Mangaka || f.SenderId == user.Id);
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
                Type = f.Type,
                IsRead = f.IsRead,
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
                ((f.Series != null && f.Series.CreatedById == user.Id) || 
                 (f.Chapter != null && f.Chapter.Series.CreatedById == user.Id) ||
                 (f.MangaTask != null && f.MangaTask.Chapter.Series.CreatedById == user.Id)) 
                && (f.Sender.Role == UserRole.Tantou || f.Sender.Role == UserRole.Editorial || f.SenderId == user.Id)
            );
        }
        else if (user.Role == UserRole.Tantou)
        {
            query = query.Where(f => 
                ((f.Series != null && f.Series.CreatedBy.SupervisorId == user.Id) || 
                 (f.Chapter != null && f.Chapter.Series.CreatedBy.SupervisorId == user.Id) ||
                 (f.MangaTask != null && f.MangaTask.Chapter.Series.CreatedBy.SupervisorId == user.Id))
                && (f.Sender.Role == UserRole.Editorial || f.Sender.Role == UserRole.Mangaka || f.SenderId == user.Id)
            );
        }
        else if (user.Role == UserRole.Editorial)
        {
            query = query.Where(f => f.SenderId == user.Id);
        }
        else if (user.Role == UserRole.Assistant)
        {
            query = query.Where(f => f.MangaTask != null && f.MangaTask.AssignedToId == user.Id && 
                                     f.Sender.Role == UserRole.Mangaka);
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
                Type = f.Type,
                IsRead = f.IsRead,
                CreatedAt = f.CreatedAt
            }
        ).ToListAsync();
        
        return feedback;
    }

    public async Task<bool> MarkAsRead(Guid feedbackId)
    {
        var userId = GetUserIdCurrent();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null || user.IsDeleted) throw new UnauthorizedAccessException("Unauthorized");

        var feedback = await _dbContext.Feedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId && !f.IsDeleted);
        if (feedback == null) throw new KeyNotFoundException("Feedback not found");

        feedback.IsRead = true;
        _dbContext.Feedbacks.Update(feedback);
        await _dbContext.SaveChangesAsync();
        return true;
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