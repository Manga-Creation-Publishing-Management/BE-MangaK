using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Manga.Repository.Data;
using Microsoft.EntityFrameworkCore;
using ChapterVotingService = Manga.Service.ChapterVoting.IService;

namespace Manga.Service.Leaderboard;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    private static (DateTime Start, DateTime End) GetLastWeekPeriod()
    {
        var today = DateTime.UtcNow.Date;

        // Monday tuần hiện tại
        var currentMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

        if (today.DayOfWeek == DayOfWeek.Sunday)
            currentMonday = today.AddDays(-6);

        var lastMonday = currentMonday.AddDays(-7);

        return (lastMonday, currentMonday);
    }
    
    public async Task GenerateWeeklyLeaderboard()
    {
        var (start, end) = GetLastWeekPeriod();

        bool existed = await _dbContext.Leaderboards.AnyAsync(x =>
            // x.Type == LeaderboardType.Weekly &&
            x.PeriodStart.Date == start.Date &&
            x.PeriodEnd.Date == end.Date);

        if (existed)
            return;

        var rankings = await _dbContext.Series
            .Where(x => x.Status == SeriesStatus.Publishing && !x.IsDeleted)
            .Select(series => new
            {
                Series = series,

                Votes = series.Chapters.Where(c=>!c.IsDeleted && c.Status == ChapterStatus.Publishing)
                    .SelectMany(c => c.ChapterVotes)
                    .Where(v =>
                        !v.IsDeleted &&
                        v.VoteAt >= start &&
                        v.VoteAt < end)
                    .ToList()
            })
            .ToListAsync();
        
        var result = rankings.Select(x => new
            {
                x.Series,
                TotalVotes = x.Votes.Count,
                AverageRate = x.Votes.Any() ? Math.Round(x.Votes.Average(v=>v.Rate),2) :0
            })
            .Where(x => x.TotalVotes >= 10)
            .OrderByDescending(x => x.AverageRate)
            .ThenByDescending(x => x.TotalVotes)
            .ToList();
        
        int rank = 1;

        foreach (var item in result)
        {
            _dbContext.Leaderboards.Add(new Repository.Entity.Leaderboard
            {
                Id = Guid.NewGuid(),
                // Type = LeaderboardType.Weekly,
                PeriodStart = start,
                PeriodEnd = end,
                RankPosition = rank++,
                SeriesId = item.Series.Id,
                TotalVotes = item.TotalVotes,
                AverageRate = item.AverageRate,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
    }

    private static (DateTime Start, DateTime End) GetLastMonthPeriod()
    {
        var now = DateTime.UtcNow;

        DateTime end;

        if (now.Day >= 25)
        {
            end = new DateTime(now.Year, now.Month, 25);
        }
        else
        {
            var prev = now.AddMonths(-1);
            end = new DateTime(prev.Year, prev.Month, 25);
        }

        var start = end.AddMonths(-1);

        return (start, end);
    }
    
    public async Task GenerateMonthlyLeaderboard()
    {
        var (start, end) = GetLastMonthPeriod();

        bool existed = await _dbContext.Leaderboards.AnyAsync(x =>
            // x.Type == LeaderboardType.Monthly &&
            x.PeriodStart.Date == start.Date &&
            x.PeriodEnd.Date == end.Date);

        if (existed)
            return;

        var rankings = await _dbContext.Series
            .Where(x => x.Status == SeriesStatus.Publishing && !x.IsDeleted)
            .Select(series => new
            {
                Series = series,

                Votes = series.Chapters.Where(c=>!c.IsDeleted && c.Status == ChapterStatus.Publishing)
                    .SelectMany(c => c.ChapterVotes)
                    .Where(v =>
                        !v.IsDeleted &&
                        v.VoteAt >= start &&
                        v.VoteAt < end)
                    .ToList()
            })
            .ToListAsync();
        
        var result = rankings.Select(x => new
            {
                x.Series,
                TotalVotes = x.Votes.Count,
                AverageRate = x.Votes.Any() ? Math.Round(x.Votes.Average(v=>v.Rate),2) :0
            })
            .Where(x => x.TotalVotes >= 10)
            .OrderByDescending(x => x.AverageRate)
            .ThenByDescending(x => x.TotalVotes)
            .ToList();
        
        int rank = 1;

        foreach (var item in result)
        {
            _dbContext.Leaderboards.Add(new Repository.Entity.Leaderboard
            {
                Id = Guid.NewGuid(),
                // Type = LeaderboardType.Monthly,
                PeriodStart = start,
                PeriodEnd = end,
                RankPosition = rank++,
                SeriesId = item.Series.Id,
                TotalVotes = item.TotalVotes,
                AverageRate = item.AverageRate,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}