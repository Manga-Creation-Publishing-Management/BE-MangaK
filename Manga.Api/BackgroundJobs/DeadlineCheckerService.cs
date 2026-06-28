using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Manga.Api.BackgroundJobs;

public class DeadlineCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeadlineCheckerService> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    public DeadlineCheckerService(IServiceProvider serviceProvider, ILogger<DeadlineCheckerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndUpdateStatusesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking deadlines.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckAndUpdateStatusesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTimeOffset.UtcNow;
        
        await CheckPublishingSchedulesAsync(dbContext, now);
        await CheckChapterDeadlinesAsync(dbContext, now);
        await CheckMangaTaskDeadlinesAsync(dbContext, now);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task CheckPublishingSchedulesAsync(AppDbContext dbContext, DateTimeOffset now)
    {
        var schedulesToPublish = await dbContext.PublishingSchedules
            .Include(p => p.Series)
            .Where(p => p.Series.Status == SeriesStatus.Scheduled
                        && p.PublishDate <= now)
            .ToListAsync();

        foreach (var schedule in schedulesToPublish)
        {
            schedule.Series.Status    = SeriesStatus.Publishing;
            schedule.Series.UpdatedAt = now;

            _logger.LogInformation("Series {SeriesId} switched to Publishing.", schedule.SeriesId);
        }
    }
    
    private async Task CheckChapterDeadlinesAsync(AppDbContext dbContext, DateTimeOffset now)
    {
        var chaptersToUpdate = await dbContext.Chapters
            .Where(c => !c.IsDeleted
                        && (c.Status == ChapterStatus.Created || c.Status == ChapterStatus.Processing)
                        && c.Deadline <= now)
            .ToListAsync();

        foreach (var chapter in chaptersToUpdate)
        {
            chapter.Status    = ChapterStatus.Pending;
            chapter.UpdatedAt = now;
        }

        if (chaptersToUpdate.Count > 0)
            _logger.LogInformation("{Count} chapter(s) switched to Pending.", chaptersToUpdate.Count);
    }
    
    private async Task CheckMangaTaskDeadlinesAsync(AppDbContext dbContext, DateTimeOffset now)
    {
        var tasksToUpdate = await dbContext.MangaTasks
            .Where(t => !t.IsDeleted
                        && t.Status == MangaTaskStatus.Processing
                        && t.Deadline <= now)
            .ToListAsync();

        foreach (var task in tasksToUpdate)
        {
            task.Status    = MangaTaskStatus.Pending;
            task.UpdatedAt = now;
        }

        if (tasksToUpdate.Count > 0)
            _logger.LogInformation("{Count} manga task(s) switched to Pending.", tasksToUpdate.Count);
    }
}