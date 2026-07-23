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

                var now = DateTime.UtcNow;

                // Weekly: Sau 00:00 Thứ Hai sẽ generate
                if (now.DayOfWeek == DayOfWeek.Monday)
                {
                    await leaderboardService.GenerateWeeklyLeaderboard();

                    _logger.LogInformation("Checked weekly leaderboard.");
                }

                // Monthly: Sau 00:00 ngày 25 sẽ generate
                if (now.Day == 25)
                {
                    await leaderboardService.GenerateMonthlyLeaderboard();

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