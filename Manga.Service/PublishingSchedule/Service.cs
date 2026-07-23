using MailKit.Net.Imap;
using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.PublishingSchedule;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response.CreatePublishingScheduleResponse> CreatePublishingSchedule(Guid seriesId, Request.CreatePublishingScheduleRequest request)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type == "userId"|| x.Type == "UserId")?.Value;

        if (userId == null)
            throw new UnauthorizedAccessException("User not login");
        
        var userIdGuid = Guid.Parse(userId);

        var board = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userIdGuid && !u.IsDeleted && u.Status == UserStatus.Active);
        
        if(board == null)
            throw new UnauthorizedAccessException("User not found");

        if (board.Role != UserRole.Editorial)
            throw new UnauthorizedAccessException("Only Board must can create PublishSchedule.");

        var series = await _dbContext.Series
            .Include(s => s.PublishingSchedule)
            .FirstOrDefaultAsync(s => s.Id == seriesId && !s.IsDeleted);
        
        if(series == null)
            throw new KeyNotFoundException("Series not found");

        if (series.Status != SeriesStatus.Approved)
            throw new InvalidOperationException($"Series must be in approved status. Current status{series.Status}");

        if (series.PublishingSchedule != null && !series.PublishingSchedule.IsDeleted)
            throw new InvalidOperationException("Publishing Schedule already exists");
        
        if(request.PublishDate <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Publish date must be in the future");
 
        Deadline.DeadlineCalculator.ValidatePublishDate(request.PublishDate, request.PublishPeriod);

        var schedule = new Repository.Entity.PublishingSchedule
        {
            Id = Guid.NewGuid(),
            PublishDate = request.PublishDate,
            PublishPeriod = request.PublishPeriod,
            SeriesId = seriesId,
            DecidedById = userIdGuid,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.PublishingSchedules.AddAsync(schedule);

        series.Status = SeriesStatus.Scheduled;
        series.UpdatedAt = DateTimeOffset.UtcNow;

        var statusChangeFeedback = new Repository.Entity.Feedback
        {
            Id        = Guid.NewGuid(),
            SenderId  = userIdGuid,
            Content   = "Editorial Board has scheduled series " + series.Title,
            SeriesId  = series.Id,
            Type      = FeedbackType.StatusChange,
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead    = false
        };
        await _dbContext.Feedbacks.AddAsync(statusChangeFeedback);

        await _dbContext.SaveChangesAsync();

        return new Response.CreatePublishingScheduleResponse()
        {
            ScheduleId = schedule.Id,
            SeriesId = seriesId,
            SeriesTitle = series.Title,
            SeriesStatus = series.Status,
            PublishDate = request.PublishDate,
            PublishPeriod = request.PublishPeriod,
            DecidedByName = $"{board.FirstName}{board.LastName}",
            CreateAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<List<Response.GetPublishingScheduleResponse>> GetAllPublishingSchedules()
    {
        var userIdStr = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(c => c.Type == "userId" || c.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(userIdStr))
            throw new UnauthorizedAccessException("User not login");

        var userIdGuid = Guid.Parse(userIdStr);
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid && !u.IsDeleted && u.Status == UserStatus.Active);

        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        var allowedRoles = new[]
        {
            UserRole.Editorial,
            UserRole.Tantou,
            UserRole.Admin
        };
        
        if(!allowedRoles.Contains(user.Role))
            throw new UnauthorizedAccessException("Only EditorialBoard, TantouEditor, Admin can view all publishing schedules.");

        var schedule = await _dbContext.PublishingSchedules
            .Where(p => !p.IsDeleted)
            .Include(p => p.Series)
            .ThenInclude(s => s.CreatedBy)
            .Include(p => p.DecidedBy)
            .OrderBy(p => p.PublishDate)
            .ToListAsync();

        return schedule.Select(p => new Response.GetPublishingScheduleResponse()
        {
            ScheduleId      = p.Id,
            SeriesId        = p.SeriesId,
            SeriesTitle     = p.Series.Title,
            SeriesCoverFile = p.Series.CoverFile,
            SeriesStatus    = p.Series.Status,
            MangakaName     = p.Series.CreatedBy.AuthorName ??
                          $"{p.Series.CreatedBy.FirstName}{p.Series.CreatedBy.LastName}",
            PublishDate     = p.PublishDate,
            PublishPeriod   = p.PublishPeriod,
            DecidedByName   = p.DecidedBy != null ? $"{p.DecidedBy.FirstName}{p.DecidedBy.LastName}" : string.Empty,
            CreatedAt       = p.CreatedAt,
            UpdatedAt       = p.UpdatedAt
        }).ToList();    
    }

    public async Task<Response.GetPublishingScheduleResponse> UpdatePublishingSchedule(Guid scheduleId, Request.UpdatePublishingScheduleRequest request)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")!.Value;
        
        if(userId == null)
            throw new UnauthorizedAccessException("User not login");

        var userIdGuid = Guid.Parse(userId);

        var user = _dbContext.Users.FirstOrDefault(u => u.Id == userIdGuid && !u.IsDeleted && u.Status == UserStatus.Active);
        
        if(user == null)
            throw new UnauthorizedAccessException("User not found");

        if (user.Role != UserRole.Editorial)
            throw new UnauthorizedAccessException("Only EditorialBoard must be update publish schedule.");
        
        var schedule =
            await _dbContext.PublishingSchedules
                .Where(p => p.Id == scheduleId)
                .Include(p => p.Series)
                .ThenInclude(s => s.CreatedBy)
                .Include(p => p.Series)
                .ThenInclude(s => s.Chapters.Where(c => !c.IsDeleted))
                .Include(p => p.DecidedBy)
                .OrderBy(p => p.PublishDate)
                .FirstOrDefaultAsync();

        if(schedule == null)
            throw new KeyNotFoundException("Publishing schedule not found");
        
        //
        if(schedule.Series.Status != SeriesStatus.Scheduled)
            throw new InvalidOperationException($"Series must be in scheduled  status. Current status{schedule.Series.Status}");
        
        var newPublishPeriod = request.PublishPeriod ?? schedule.PublishPeriod;
        
        Deadline.DeadlineCalculator.ValidatePublishDate(request.PublishDate, newPublishPeriod);
        
        schedule.PublishDate = request.PublishDate;
        
        if(request.PublishDate <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Publish must be in the future");

        schedule.PublishDate = request.PublishDate;

        if (request.PublishPeriod != null)
            schedule.PublishPeriod = request.PublishPeriod;
        
        schedule.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _dbContext.SaveChangesAsync();

        var chaptersToRecalculate = schedule.Series.Chapters
            .Where(c => c.Status == ChapterStatus.Created || c.Status == ChapterStatus.Processing)
            .ToList();

        foreach (var chapter in chaptersToRecalculate)
        {
            chapter.Deadline = Deadline.DeadlineCalculator.CalculateDeadline(
                schedule.PublishDate,
                schedule.PublishPeriod,
                chapter.ChapterNumber);

            chapter.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        
        return new Response.GetPublishingScheduleResponse()
        {
            ScheduleId = schedule.Id,
            SeriesId = schedule.Series.Id,
            SeriesTitle = schedule.Series.Title,
            SeriesCoverFile = schedule.Series.CoverFile,
            SeriesStatus = schedule.Series.Status,
            MangakaName = schedule.Series.CreatedBy.AuthorName
                          ?? $"{schedule.Series.CreatedBy.FirstName} {schedule.Series.CreatedBy.LastName}",
            PublishDate = schedule.PublishDate,
            PublishPeriod = schedule.PublishPeriod,
            DecidedByName = schedule.DecidedBy != null
                ? $"{schedule.DecidedBy.FirstName} {schedule.DecidedBy.LastName}"
                : string.Empty,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }

    public async Task DeletePublishingSchedule(Guid scheduleId)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")!.Value;
        
        if(userId == null)
            throw new UnauthorizedAccessException("User not login");

        var userIdGuid = Guid.Parse(userId);

        var user = _dbContext.Users.FirstOrDefault(u => u.Id == userIdGuid && !u.IsDeleted && u.Status == UserStatus.Active);
        
        if(user == null)
            throw new UnauthorizedAccessException("User not found");

        if (user.Role != UserRole.Editorial)
            throw new UnauthorizedAccessException("Only EditorialBoard must be update publish schedule.");
        
        var schedule =
            await _dbContext.PublishingSchedules
                .Where(p => p.Id == scheduleId)
                .Include(p => p.Series)
                .ThenInclude(s => s.CreatedBy)
                .Include(p => p.DecidedBy)
                .OrderBy(p => p.PublishDate)
                .FirstOrDefaultAsync();

        if(schedule == null)
            throw new KeyNotFoundException("Publishing schedule not found");
        
        schedule.IsDeleted = true;
        schedule.UpdatedAt = DateTimeOffset.UtcNow;
        
        schedule.Series.Status = SeriesStatus.Approved;
        schedule.UpdatedAt = DateTimeOffset.UtcNow;
        
        var statusChangeFeedback = new Repository.Entity.Feedback
        {
            Id        = Guid.NewGuid(),
            SenderId  = userIdGuid,
            Content   = "System has reverted series " + schedule.Series.Title + " to Approved",
            SeriesId  = schedule.Series.Id,
            Type      = FeedbackType.StatusChange,
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead    = false
        };
        await _dbContext.Feedbacks.AddAsync(statusChangeFeedback);
        
        await _dbContext.SaveChangesAsync();
    }
}