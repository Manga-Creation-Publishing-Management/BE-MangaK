using Manga.Repository.Data;
using Microsoft.EntityFrameworkCore;
using ChapterVotingService = Manga.Service.ChapterVoting.IService;

namespace Manga.Service.Leaderboard;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ChapterVotingService _chapterVotingService;

    public Service(AppDbContext dbContext, ChapterVotingService chapterVotingService)
    {
        _dbContext = dbContext;
        _chapterVotingService = chapterVotingService;
    }

    public async Task<IEnumerable<Response.LeaderboardResponse>> GetWeeklyLeaderboard()
    {
        var rankingResponse = await _chapterVotingService.CalculateChapterVote();
        var weeklyRanking = rankingResponse.WeeklyRanking;
        if (weeklyRanking == null || !weeklyRanking.Any()) 
            return new List<Response.LeaderboardResponse>();
        var seriesIds = weeklyRanking.Select(r => r.SeriesId).ToList();

        var authorInfo = await _dbContext.Series
            .Where(s => seriesIds.Contains(s.Id))
            .Select(s => new 
            {
                SeriesId = s.Id,
                AuthorName = !string.IsNullOrEmpty(s.CreatedBy.AuthorName) 
                    ? s.CreatedBy.AuthorName 
                    : (s.CreatedBy.FirstName + " " + s.CreatedBy.LastName).Trim()
            })
            .ToDictionaryAsync(x => x.SeriesId, x => x.AuthorName);

        var result = new List<Response.LeaderboardResponse>();

        foreach (var current in weeklyRanking)
        {
            var authorName = authorInfo.GetValueOrDefault(current.SeriesId) ?? "Unknown";

            var prevPeriodStart = current.WeeklyPeriodStart.AddDays(-7);
            var prevPeriodEnd = current.WeeklyPeriodStart;

            int prevVotes = await _dbContext.Chapters
                .Where(c => c.SeriesId == current.SeriesId && !c.IsDeleted)
                .SelectMany(c => c.ChapterVotes)
                .CountAsync(v => v.VoteAt >= prevPeriodStart && v.VoteAt < prevPeriodEnd);

            int currentVotesCount = current.WeeklyTotalVotes;
            double currentAverageRate = current.WeeklyAverageRate;

            string changeStr = "0.0%";
            if (prevVotes > 0)
            {
                double change = ((double)(currentVotesCount - prevVotes) / prevVotes) * 100;
                changeStr = (change > 0 ? "+" : "") + change.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%";
            }
            else if (currentVotesCount > 0)
            {
                changeStr = "+100.0%";
            }

            result.Add(new Response.LeaderboardResponse
            {
                Series = current.Title,
                Author = authorName,
                Votes = currentVotesCount,
                AverageRate = currentAverageRate,
                Change = changeStr
            });
        }

        result = result.OrderByDescending(r => r.Votes).ToList();
        
        int rank = 1;
        foreach (var item in result)
        {
            item.Rank = rank++;
        }

        return result;
    }

    public async Task<IEnumerable<Response.LeaderboardResponse>> GetMonthlyLeaderboard()
    {
        var rankingResponse = await _chapterVotingService.CalculateChapterVote();
        var monthlyRanking = rankingResponse.MonthlyRanking;

        if (monthlyRanking == null || !monthlyRanking.Any()) 
            return new List<Response.LeaderboardResponse>();

        var seriesIds = monthlyRanking.Select(r => r.SeriesId).ToList();

        var authorInfo = await _dbContext.Series
            .Where(s => seriesIds.Contains(s.Id))
            .Select(s => new 
            {
                SeriesId = s.Id,
                AuthorName = !string.IsNullOrEmpty(s.CreatedBy.AuthorName) 
                    ? s.CreatedBy.AuthorName 
                    : (s.CreatedBy.FirstName + " " + s.CreatedBy.LastName).Trim()
            })
            .ToDictionaryAsync(x => x.SeriesId, x => x.AuthorName);

        var result = new List<Response.LeaderboardResponse>();

        foreach (var current in monthlyRanking)
        {
            var authorName = authorInfo.GetValueOrDefault(current.SeriesId) ?? "Unknown";

            var prevPeriodStart = current.MonthlyPeriodStart.AddDays(-30);
            var prevPeriodEnd = current.MonthlyPeriodStart;

            int prevVotes = await _dbContext.Chapters
                .Where(c => c.SeriesId == current.SeriesId && !c.IsDeleted)
                .SelectMany(c => c.ChapterVotes)
                .CountAsync(v => v.VoteAt >= prevPeriodStart && v.VoteAt < prevPeriodEnd);

            int currentVotesCount = current.MonthlyTotalVotes;
            double currentAverageRate = current.MonthlyAverageRate;

            string changeStr = "0.0%";
            if (prevVotes > 0)
            {
                double change = ((double)(currentVotesCount - prevVotes) / prevVotes) * 100;
                changeStr = (change > 0 ? "+" : "") + change.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%";
            }
            else if (currentVotesCount > 0)
            {
                changeStr = "+100.0%";
            }

            result.Add(new Response.LeaderboardResponse
            {
                Series = current.Title,
                Author = authorName,
                Votes = currentVotesCount,
                AverageRate = currentAverageRate,
                Change = changeStr
            });
        }

        result = result.OrderByDescending(r => r.Votes).ToList();
        
        int rank = 1;
        foreach (var item in result)
        {
            item.Rank = rank++;
        }

        return result;
    }
}