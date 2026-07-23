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
       await CheckScheduledChaptersAsync(dbContext, now);
       await CheckMangaTaskDeadlinesAsync(dbContext, now);


       await CheckSeriesDelaysAsync(dbContext, now);
       await CheckChapterTaskCreationAsync(dbContext, now);
       await CheckTaskAcceptanceAndReviewDelaysAsync(dbContext, now);


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
  
   //
   private async Task CheckScheduledChaptersAsync(AppDbContext dbContext, DateTimeOffset now)
   {
       var scheduledChapters = await dbContext.Chapters
           .Where(c => !c.IsDeleted && c.Status == ChapterStatus.Scheduled)
           .Include(c => c.Series)
               .ThenInclude(s => s.PublishingSchedule)
           .ToListAsync();


       foreach (var chapter in scheduledChapters)
       {
           var schedule = chapter.Series.PublishingSchedule;
          
           if (schedule == null || schedule.IsDeleted)
               continue;
          
           var publishDateForChapter = CalculateChapterPublishDate(
               schedule.PublishDate,
               schedule.PublishPeriod,
               chapter.ChapterNumber);
          
           if (now >= publishDateForChapter)
           {
               chapter.Status    = ChapterStatus.Publishing;
               chapter.UpdatedAt = now;


               _logger.LogInformation(
                   "Chapter {ChapterNumber} of Series {SeriesId} switched to Publishing.",
                   chapter.ChapterNumber, chapter.SeriesId);
           }
       }
   }
  
   private static DateTimeOffset CalculateChapterPublishDate(
       DateTimeOffset publishDate,
       string? publishPeriod,
       int chapterNumber)
   {
       var periodDays = publishPeriod?.Trim().ToLowerInvariant() switch
       {
           "weekly"  => 7,
           "monthly" => 30,
           _         => 7 
       };


       return publishDate.AddDays((chapterNumber - 1) * periodDays);
   }
   //
  
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
  
   private async Task<Guid?> GetSystemAdminUserIdAsync(AppDbContext dbContext)
   {
       var systemAdmin = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "mangaksystem.admin@gmail.com");
       return systemAdmin?.Id;
   }


   private async Task CheckSeriesDelaysAsync(AppDbContext dbContext, DateTimeOffset now)
   {
       var boardUserId = await GetSystemAdminUserIdAsync(dbContext);
       if (boardUserId == null) return;


       var sevenDaysAgo = now.AddDays(-7);


       var seriesToTantou = await dbContext.Series
           .Where(s => s.Status == SeriesStatus.Processing && (s.UpdatedAt ?? s.CreatedAt) <= sevenDaysAgo)
           .ToListAsync();


       foreach (var series in seriesToTantou)
       {
           var alreadyNotified = await dbContext.Feedbacks.AnyAsync(f => f.SeriesId == series.Id && f.Content == "Reminder: Series is waiting for Tantou review." && f.CreatedAt >= sevenDaysAgo);
           if (!alreadyNotified)
           {
               dbContext.Feedbacks.Add(new Repository.Entity.Feedback
               {
                   Id = Guid.NewGuid(),
                   SeriesId = series.Id,
                   SenderId = boardUserId.Value,
                   Content = "Reminder: Series is waiting for Tantou review.",
                   Type = FeedbackType.StatusChange,
                   CreatedAt = now,
                   IsRead = false
               });
           }
       }


       var seriesToBoard = await dbContext.Series
           .Where(s => s.Status == SeriesStatus.Pending && (s.UpdatedAt ?? s.CreatedAt) <= sevenDaysAgo)
           .ToListAsync();


       foreach (var series in seriesToBoard)
       {
           var alreadyNotified = await dbContext.Feedbacks.AnyAsync(f => f.SeriesId == series.Id && f.Content == "Reminder: Series is waiting for Board review." && f.CreatedAt >= sevenDaysAgo);
           if (!alreadyNotified)
           {
               dbContext.Feedbacks.Add(new Repository.Entity.Feedback
               {
                   Id = Guid.NewGuid(),
                   SeriesId = series.Id,
                   SenderId = boardUserId.Value,
                   Content = "Reminder: Series is waiting for Board review.",
                   Type = FeedbackType.StatusChange,
                   CreatedAt = now,
                   IsRead = false
               });
           }
       }


       var seriesWaitingSchedule = await dbContext.Series
           .Include(s => s.PublishingSchedule)
           .Where(s => s.Status == SeriesStatus.Approved && s.PublishingSchedule == null && (s.UpdatedAt ?? s.CreatedAt) <= sevenDaysAgo)
           .ToListAsync();


       foreach (var series in seriesWaitingSchedule)
       {
           var alreadyNotified = await dbContext.Feedbacks.AnyAsync(f => f.SeriesId == series.Id && f.Content == "Reminder: Series is waiting for Schedule creation." && f.CreatedAt >= sevenDaysAgo);
           if (!alreadyNotified)
           {
               dbContext.Feedbacks.Add(new Repository.Entity.Feedback
               {
                   Id = Guid.NewGuid(),
                   SeriesId = series.Id,
                   SenderId = boardUserId.Value,
                   Content = "Reminder: Series is waiting for Schedule creation.",
                   Type = FeedbackType.StatusChange,
                   CreatedAt = now,
                   IsRead = false
               });
           }
       }
   }


   private async Task CheckChapterTaskCreationAsync(AppDbContext dbContext, DateTimeOffset now)
   {
       var boardUserId = await GetSystemAdminUserIdAsync(dbContext);
       if (boardUserId == null) return;


       var twelveHoursAgo = now.AddHours(-12);


       var chapters = await dbContext.Chapters
           .Include(c => c.MangaTasks)
           .Where(c => !c.IsDeleted && c.CreatedAt <= twelveHoursAgo && c.MangaTasks.Count == 0)
           .ToListAsync();


       foreach (var chapter in chapters)
       {
           var alreadyNotified = await dbContext.Feedbacks.AnyAsync(f => f.ChapterId == chapter.Id && f.Content == "Reminder: Please create a task for this chapter." && f.CreatedAt >= now.AddDays(-1));
           if (!alreadyNotified)
           {
               dbContext.Feedbacks.Add(new Repository.Entity.Feedback
               {
                   Id = Guid.NewGuid(),
                   ChapterId = chapter.Id,
                   SeriesId = chapter.SeriesId,
                   SenderId = boardUserId.Value,
                   Content = "Reminder: Please create a task for this chapter.",
                   Type = FeedbackType.StatusChange,
                   CreatedAt = now,
                   IsRead = false
               });
           }
       }
   }


   private async Task CheckTaskAcceptanceAndReviewDelaysAsync(AppDbContext dbContext, DateTimeOffset now)
   {
       var boardUserId = await GetSystemAdminUserIdAsync(dbContext);
       if (boardUserId == null) return;


       var twentyFourHoursAgo = now.AddHours(-24);


       var tasksToAccept = await dbContext.MangaTasks
           .Where(t => !t.IsDeleted && t.Status == MangaTaskStatus.Available && t.CreatedAt <= twentyFourHoursAgo)
           .ToListAsync();


       foreach (var task in tasksToAccept)
       {
           task.Status = MangaTaskStatus.Rejected;
           task.UpdatedAt = now;


           dbContext.Feedbacks.Add(new Repository.Entity.Feedback
           {
               Id = Guid.NewGuid(),
               MangaTaskId = task.Id,
               ChapterId = task.ChapterId,
               SenderId = boardUserId.Value,
               Content = "Task was rejected automatically due to no action from assistant in 24 hours. Please re-assign.",
               Type = FeedbackType.StatusChange,
               CreatedAt = now,
               IsRead = false
           });
       }


       var tasksToReview = await dbContext.MangaTasks
           .Include(t => t.Chapter)
           .ThenInclude(c => c.Series)
           .ThenInclude(s => s.PublishingSchedule)
           .Where(t => !t.IsDeleted && t.Status == MangaTaskStatus.Pending)
           .ToListAsync();


       foreach (var task in tasksToReview)
       {
           var publishPeriod = task.Chapter?.Series?.PublishingSchedule?.PublishPeriod;
           var maxReviewHours = publishPeriod?.Equals("Weekly", StringComparison.OrdinalIgnoreCase) == true ? 24 : 48;
          
           var submittedTime = task.UpdatedAt ?? task.SubmittedAt ?? task.CreatedAt;
           var deadline = submittedTime.AddHours(maxReviewHours);


           if (now >= deadline)
           {
               var alreadyNotified = await dbContext.Feedbacks.AnyAsync(f => f.MangaTaskId == task.Id && f.Content == "Reminder: Please review this submitted task." && f.CreatedAt >= now.AddDays(-1));
               if (!alreadyNotified)
               {
                   dbContext.Feedbacks.Add(new Repository.Entity.Feedback
                   {
                       Id = Guid.NewGuid(),
                       MangaTaskId = task.Id,
                       ChapterId = task.ChapterId,
                       SenderId = boardUserId.Value,
                       Content = "Reminder: Please review this submitted task.",
                       Type = FeedbackType.StatusChange,
                       CreatedAt = now,
                       IsRead = false
                   });
               }
           }
       }
   }
}

