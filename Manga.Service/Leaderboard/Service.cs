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

    public async Task<IEnumerable<Response.LeaderboardResponse>> GetWeeklyLeaderboardAsync()
    {
        var chapterVotes = await _chapterVotingService.CalculateChapterVote();
        
        var ranked = chapterVotes
            .OrderByDescending(x => x.WeeklyAverageRate)
            .ThenByDescending(x => x.WeeklyTotalVotes)
            .ToList();

        return await MapToLeaderboardResponse(ranked, true);
    }

    public async Task<IEnumerable<Response.LeaderboardResponse>> GetMonthlyLeaderboardAsync()
    {
        var chapterVotes = await _chapterVotingService.CalculateChapterVote();
        
        var ranked = chapterVotes
            .OrderByDescending(x => x.MonthlyAverageRate)
            .ThenByDescending(x => x.MonthlyTotalVotes)
            .ToList();

        return await MapToLeaderboardResponse(ranked, false);
    }

    private async Task<IEnumerable<Response.LeaderboardResponse>> MapToLeaderboardResponse(List<ChapterVoting.Response.SeriesRankingResponse> rankings, bool isWeekly)
    {
        var response = new List<Response.LeaderboardResponse>();
        
        var seriesIds = rankings.Select(r => r.SeriesId).ToList();
        var seriesData = await _dbContext.Series
            .Include(s => s.CreatedBy)
            .Include(s => s.Chapters)
                .ThenInclude(c => c.ChapterVotes)
            .Where(s => seriesIds.Contains(s.Id))
            .ToListAsync();

        for (int i = 0; i < rankings.Count; i++)
        {
            var item = rankings[i];
            var series = seriesData.FirstOrDefault(s => s.Id == item.SeriesId);
            var authorName = series?.CreatedBy?.AuthorName ?? (series?.CreatedBy?.FirstName + " " + series?.CreatedBy?.LastName);
            
            string changeStr = "0.0%";
            if (series != null)
            {
                int previousVotesCount = 0;
                int currentVotesCount = isWeekly ? item.WeeklyTotalVotes : item.MonthlyTotalVotes;

                if (isWeekly)
                {
                    var previousPeriodStart = item.WeeklyPeriodStart.AddDays(-7);
                    var previousPeriodEnd = item.WeeklyPeriodStart;
                    previousVotesCount = series.Chapters.SelectMany(c => c.ChapterVotes)
                        .Count(v => v.VoteAt >= previousPeriodStart && v.VoteAt < previousPeriodEnd);
                }
                else
                {
                    var previousPeriodStart = item.MonthlyPeriodStart.AddDays(-30);
                    var previousPeriodEnd = item.MonthlyPeriodStart;
                    previousVotesCount = series.Chapters.SelectMany(c => c.ChapterVotes)
                        .Count(v => v.VoteAt >= previousPeriodStart && v.VoteAt < previousPeriodEnd);
                }

                if (previousVotesCount > 0)
                {
                    double change = (double)(currentVotesCount - previousVotesCount) / previousVotesCount * 100;
                    changeStr = (change > 0 ? "+" : "") + change.ToString("0.0") + "%";
                }
                else if (currentVotesCount > 0)
                {
                    changeStr = "+100.0%";
                }
            }

            response.Add(new Response.LeaderboardResponse
            {
                Rank = i + 1,
                Series = item.Title,
                Author = authorName ?? "Unknown",
                Votes = isWeekly ? item.WeeklyTotalVotes : item.MonthlyTotalVotes,
                Ratevote = isWeekly ? item.WeeklyAverageRate : item.MonthlyAverageRate,
                Change = changeStr
            });
        }

        return response;
    }
}