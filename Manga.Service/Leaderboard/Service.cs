using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Manga.Repository.Data;
using Microsoft.EntityFrameworkCore;
using ChapterVotingService = Manga.Service.ChapterVoting.IService;
using Microsoft.EntityFrameworkCore;

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
    
    public async Task<bool> GenerateWeeklyLeaderboard()
    {
        var (start, end) = GetLastWeekPeriod();

        bool existed = await _dbContext.Leaderboards.AnyAsync(x =>
            // x.Type == LeaderboardType.Weekly &&
            x.PeriodStart.Date == start.Date &&
            x.PeriodEnd.Date == end.Date);

        if (existed)
            return false;

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
        return true;
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
    
    public async Task<bool> GenerateMonthlyLeaderboard()
    {
        var (start, end) = GetLastMonthPeriod();

        bool existed = await _dbContext.Leaderboards.AnyAsync(x =>
            // x.Type == LeaderboardType.Monthly &&
            x.PeriodStart.Date == start.Date &&
            x.PeriodEnd.Date == end.Date);

        if (existed)
            return false;

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
        return true;
    }

    public async Task<List<string>> GetAvailablePeriods(string type)
    {
        var periods = await _dbContext.Leaderboards
            .Select(x => new { x.PeriodStart, x.PeriodEnd })
            .Distinct()
            .ToListAsync();

        var filtered = periods.Where(p => 
        {
            var diff = p.PeriodEnd - p.PeriodStart;
            if (type.Equals("weekly", StringComparison.OrdinalIgnoreCase))
                return diff.TotalDays <= 7;
            if (type.Equals("monthly", StringComparison.OrdinalIgnoreCase))
                return diff.TotalDays > 7;
            return false;
        })
        .OrderByDescending(p => p.PeriodStart)
        .Select(p => $"{p.PeriodStart:yyyy-MM-dd} - {p.PeriodEnd:yyyy-MM-dd}")
        .ToList();

        return filtered;
    }

    public async Task<List<Response.LeaderboardResponse>> GetWeeklyLeaderboard(string? period)
    {
        DateTime start, end;
        if (!string.IsNullOrWhiteSpace(period))
        {
            var dates = period.Split(" - ");
            if (dates.Length != 2 || !DateTime.TryParse(dates[0], out start) || !DateTime.TryParse(dates[1], out end))
                throw new ArgumentException("Invalid period format. Expected 'yyyy-MM-dd - yyyy-MM-dd'.");
        }
        else
        {
            var latest = await _dbContext.Leaderboards
                .Where(x => x.PeriodEnd <= x.PeriodStart.AddDays(7))
                .OrderByDescending(x => x.PeriodStart)
                .Select(x => new { x.PeriodStart, x.PeriodEnd })
                .FirstOrDefaultAsync();

            if (latest != null)
            {
                start = latest.PeriodStart;
                end = latest.PeriodEnd;
            }
            else
            {
                var p = GetLastWeekPeriod();
                start = p.Start;
                end = p.End;
            }
        }

        var prevStart = start.AddDays(-7);
        var prevEnd = end.AddDays(-7);
        
        var currentLeaderboard = await _dbContext.Leaderboards
            .Include(x => x.Series)
            .ThenInclude(x => x.CreatedBy)
            .Where(x => x.PeriodStart.Date == start.Date && x.PeriodEnd.Date == end.Date)
            .OrderBy(x => x.RankPosition)
            .ToListAsync();

        var prevLeaderboardData = await _dbContext.Leaderboards
            .Where(x => x.PeriodStart.Date == prevStart.Date && x.PeriodEnd.Date == prevEnd.Date)
            .Select(x => new { x.SeriesId, x.TotalVotes })
            .ToListAsync();

        var prevLeaderboard = prevLeaderboardData
            .GroupBy(x => x.SeriesId)
            .ToDictionary(x => x.Key, x => x.First().TotalVotes);

        var result = new List<Response.LeaderboardResponse>();
        
        foreach (var item in currentLeaderboard)
        {
            var author = item.Series.CreatedBy;
            string authorName = !string.IsNullOrWhiteSpace(author.AuthorName) 
                ? author.AuthorName 
                : $"{author.FirstName} {author.LastName}".Trim();

            string change = "New";
            if (prevLeaderboard.TryGetValue(item.SeriesId, out int prevVotes) && prevVotes > 0)
            {
                double percentChange = (double)(item.TotalVotes - prevVotes) / prevVotes * 100;
                change = percentChange > 0 ? $"+{percentChange:F1}%" : $"{percentChange:F1}%";
            }
            else if (prevLeaderboard.TryGetValue(item.SeriesId, out int pv) && pv == 0 && item.TotalVotes > 0)
            {
                change = "+100%";
            }

            result.Add(new Response.LeaderboardResponse
            {
                Rank = item.RankPosition,
                Series = item.Series.Title,
                Author = authorName,
                Votes = item.TotalVotes,
                AverageRate = item.AverageRate,
                Change = change
            });
        }

        return result;
    }

    public async Task<List<Response.LeaderboardResponse>> GetMonthlyLeaderboard(string? period)
    {
        DateTime start, end;
        if (!string.IsNullOrWhiteSpace(period))
        {
            var dates = period.Split(" - ");
            if (dates.Length != 2 || !DateTime.TryParse(dates[0], out start) || !DateTime.TryParse(dates[1], out end))
                throw new ArgumentException("Invalid period format. Expected 'yyyy-MM-dd - yyyy-MM-dd'.");
        }
        else
        {
            var latest = await _dbContext.Leaderboards
                .Where(x => x.PeriodEnd > x.PeriodStart.AddDays(7))
                .OrderByDescending(x => x.PeriodStart)
                .Select(x => new { x.PeriodStart, x.PeriodEnd })
                .FirstOrDefaultAsync();

            if (latest != null)
            {
                start = latest.PeriodStart;
                end = latest.PeriodEnd;
            }
            else
            {
                var p = GetLastMonthPeriod();
                start = p.Start;
                end = p.End;
            }
        }

        var prevEnd = start;
        var prevStart = prevEnd.AddMonths(-1);
        
        var currentLeaderboard = await _dbContext.Leaderboards
            .Include(x => x.Series)
            .ThenInclude(x => x.CreatedBy)
            .Where(x => x.PeriodStart.Date == start.Date && x.PeriodEnd.Date == end.Date)
            .OrderBy(x => x.RankPosition)
            .ToListAsync();

        var prevLeaderboardData = await _dbContext.Leaderboards
            .Where(x => x.PeriodStart.Date == prevStart.Date && x.PeriodEnd.Date == prevEnd.Date)
            .Select(x => new { x.SeriesId, x.TotalVotes })
            .ToListAsync();

        var prevLeaderboard = prevLeaderboardData
            .GroupBy(x => x.SeriesId)
            .ToDictionary(x => x.Key, x => x.First().TotalVotes);

        var result = new List<Response.LeaderboardResponse>();
        
        foreach (var item in currentLeaderboard)
        {
            var author = item.Series.CreatedBy;
            string authorName = !string.IsNullOrWhiteSpace(author.AuthorName) 
                ? author.AuthorName 
                : $"{author.FirstName} {author.LastName}".Trim();

            string change = "New";
            if (prevLeaderboard.TryGetValue(item.SeriesId, out int prevVotes) && prevVotes > 0)
            {
                double percentChange = (double)(item.TotalVotes - prevVotes) / prevVotes * 100;
                change = percentChange > 0 ? $"+{percentChange:F1}%" : $"{percentChange:F1}%";
            }
            else if (prevLeaderboard.TryGetValue(item.SeriesId, out int pv) && pv == 0 && item.TotalVotes > 0)
            {
                change = "+100%";
            }

            result.Add(new Response.LeaderboardResponse
            {
                Rank = item.RankPosition,
                Series = item.Series.Title,
                Author = authorName,
                Votes = item.TotalVotes,
                AverageRate = item.AverageRate,
                Change = change
            });
        }

        return result;
    }
}