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
            .FirstOrDefaultAsync(u => u.Id == userIdGuid);
        
        if(board == null)
            throw new UnauthorizedAccessException("User not found");

        if (board.Role != UserRole.EditorialBoard)
            throw new UnauthorizedAccessException("Only Board must can create PublishSchedule.");

        var series = await _dbContext.Series
            .Include(s => s.PublishingSchedule)
            .FirstOrDefaultAsync(s => s.Id == seriesId && !s.IsDeleted);
        
        if(series == null)
            throw new KeyNotFoundException("Series not found");

        if (series.Status != SeriesStatus.Approved)
            throw new InvalidOperationException($"Series must be in approved status. Current status{series.Status}");

        if (series.PublishingSchedule != null)
            throw new InvalidOperationException("Publishing Schedule already exists");
        
        if(request.PublishDate <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Publish date must be in the future");

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

        series.Status = SeriesStatus.Publishing;
        series.UpdatedAt = DateTimeOffset.UtcNow;

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
}