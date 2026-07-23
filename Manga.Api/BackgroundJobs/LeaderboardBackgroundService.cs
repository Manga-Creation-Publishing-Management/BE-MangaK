using Manga.Repository.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Manga.Api.BackgroundJobs;

public class LeaderboardBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeaderboardBackgroundService> _logger;

    public LeaderboardBackgroundService(IServiceProvider serviceProvider, ILogger<LeaderboardBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Leaderboard Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var leaderboardService =
                    scope.ServiceProvider.GetRequiredService<Manga.Service.Leaderboard.IService>();

                var dbContext = scope.ServiceProvider.GetRequiredService<Manga.Repository.Data.AppDbContext>();

                var now = DateTime.UtcNow;

                // Weekly: Sau 00:00 Thứ Hai sẽ generate
                if (now.DayOfWeek == DayOfWeek.Monday)
                {
                    bool generated = await leaderboardService.GenerateWeeklyLeaderboard();

                    if (generated)
                    {
                        var adminUserId = await dbContext.Users
                            .Where(u => u.Email == "mangaksystem.admin@gmail.com")
                            .Select(u => u.Id)
                            .FirstOrDefaultAsync();

                        if (adminUserId != Guid.Empty)
                        {
                            var latestWeekly = await dbContext.Leaderboards
                                .Where(x => x.PeriodEnd <= x.PeriodStart.AddDays(7))
                                .OrderByDescending(x => x.PeriodStart)
                                .Select(x => x.PeriodStart)
                                .FirstOrDefaultAsync();

                            var seriesIds = await dbContext.Leaderboards
                                .Where(x => x.PeriodStart == latestWeekly)
                                .Select(x => x.SeriesId)
                                .ToListAsync();

                            foreach (var sid in seriesIds)
                            {
                                dbContext.Feedbacks.Add(new Repository.Entity.Feedback
                                {
                                    Id = Guid.NewGuid(),
                                    SenderId = adminUserId,
                                    SeriesId = sid,
                                    Content = "Congratulations! Your series has been ranked in the new Weekly Leaderboard.",
                                    Type = FeedbackType.Manual,
                                    CreatedAt = DateTimeOffset.UtcNow,
                                    IsRead = false
                                });
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    _logger.LogInformation("Checked weekly leaderboard.");
                }

                // Monthly: Sau 00:00 ngày 25 sẽ generate
                if (now.Day == 25)
                {
                    bool generated = await leaderboardService.GenerateMonthlyLeaderboard();

                    if (generated)
                    {
                        var adminUserId = await dbContext.Users
                            .Where(u => u.Email == "mangaksystem.admin@gmail.com")
                            .Select(u => u.Id)
                            .FirstOrDefaultAsync();

                        if (adminUserId != Guid.Empty)
                        {
                            var latestMonthly = await dbContext.Leaderboards
                                .Where(x => x.PeriodEnd > x.PeriodStart.AddDays(7))
                                .OrderByDescending(x => x.PeriodStart)
                                .Select(x => x.PeriodStart)
                                .FirstOrDefaultAsync();

                            var seriesIds = await dbContext.Leaderboards
                                .Where(x => x.PeriodStart == latestMonthly)
                                .Select(x => x.SeriesId)
                                .ToListAsync();

                            foreach (var sid in seriesIds)
                            {
                                dbContext.Feedbacks.Add(new Repository.Entity.Feedback
                                {
                                    Id = Guid.NewGuid(),
                                    SenderId = adminUserId,
                                    SeriesId = sid,
                                    Content = "Congratulations! Your series has been ranked in the new Monthly Leaderboard.",
                                    Type = FeedbackType.Manual,
                                    CreatedAt = DateTimeOffset.UtcNow,
                                    IsRead = false
                                });
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    _logger.LogInformation("Checked monthly leaderboard.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Leaderboard Background Service Error");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}