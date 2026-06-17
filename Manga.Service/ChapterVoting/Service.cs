using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.ChapterVoting;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response.VoteChapterResponse> VoteChapter(Request.VoteChapterRequest request)
    {
        var user = await _dbContext.Readers.FirstOrDefaultAsync(x => x.Id == GetUserCurrentId());
        if (user == null) throw new UnauthorizedAccessException("You must log in");
        var chapterExists =
            await _dbContext.Chapters.AnyAsync(x => x.Id == request.ChapterId || x.Status != ChapterStatus.Publishing);
        if (!chapterExists) throw new KeyNotFoundException("Chapter not found or already published");

        if (request.Rate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Rate));
        }

        var voting = await _dbContext.ChapterVotings
            .FirstOrDefaultAsync(x => x.ChapterId == request.ChapterId && x.ReaderId == user.Id);

        if (voting == null)
        {
            voting = new Repository.Entity.ChapterVoting()
            {
                Id = Guid.NewGuid(),
                Rate = request.Rate,
                VoteAt = DateTimeOffset.UtcNow,
                ChapterId = request.ChapterId,
                ReaderId = user.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.ChapterVotings.Add(voting);
        }
        else
        {
            voting.Rate = request.Rate;
            voting.VoteAt = DateTimeOffset.UtcNow;
            voting.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return new Response.VoteChapterResponse()
        {
            Id = voting.Id,
            ChapterId = voting.ChapterId,
            Rate = voting.Rate,
            VoteAt = voting.VoteAt
        };
    }

    public async Task<Response.RankingResponse> CalculateChapterVote()
{
    var now = DateTimeOffset.UtcNow;

    var seriesList = await _dbContext.Series
        .Where(s => s.Status == SeriesStatus.Publishing && !s.IsDeleted)
        .Include(s => s.PublishingSchedule)
        .Include(s => s.Chapters.Where(c => !c.IsDeleted))
            .ThenInclude(c => c.ChapterVotes)
        .ToListAsync();
    
    var weeklySeries  = seriesList.Where(s => s.PublishingSchedule?.PublishPeriod == "Weekly");
    var monthlySeries = seriesList.Where(s => s.PublishingSchedule?.PublishPeriod == "Monthly");

    var weeklyRanking = weeklySeries.Select(series =>
        {
            var publishDate = series.PublishingSchedule!.PublishDate;
            
            var daysPassed= (now - publishDate).TotalDays;
            
            var weeklyPeriodStart = publishDate.AddDays((int)(daysPassed / 7) * 7);
            
            var weeklyPeriodEnd   = weeklyPeriodStart.AddDays(7);
            
            var weeklyVotes = series.Chapters
                .SelectMany(c => c.ChapterVotes)
                .Where(v => v.VoteAt >= weeklyPeriodStart && v.VoteAt < weeklyPeriodEnd)
                .ToList();

            return new Response.WeeklyRankingResponse
            {
                SeriesId          = series.Id,
                Title             = series.Title,
                CoverFile         = series.CoverFile,
                WeeklyAverageRate = weeklyVotes.Count > 0 ? Math.Round(weeklyVotes.Average(v => v.Rate), 2) : 0,
                WeeklyTotalVotes  = weeklyVotes.Count,
                WeeklyPeriodStart = weeklyPeriodStart,
                WeeklyPeriodEnd   = weeklyPeriodEnd,
            };
        })
        .OrderByDescending(r => r.WeeklyAverageRate)
        .ThenByDescending(r => r.WeeklyTotalVotes)
        .ToList();

    var monthlyRanking = monthlySeries.Select(series =>
        {
            var publishDate        = series.PublishingSchedule!.PublishDate;
            
            var daysPassed         = (now - publishDate).TotalDays;
            
            var monthlyPeriodStart = publishDate.AddDays((int)(daysPassed / 30) * 30);
            
            var monthlyPeriodEnd   = monthlyPeriodStart.AddDays(30);

            var monthlyVotes = series.Chapters
                .SelectMany(c => c.ChapterVotes)
                .Where(v => v.VoteAt >= monthlyPeriodStart && v.VoteAt < monthlyPeriodEnd)
                .ToList();

            return new Response.MonthlyRankingResponse
            {
                SeriesId           = series.Id,
                Title              = series.Title,
                CoverFile          = series.CoverFile,
                MonthlyAverageRate = monthlyVotes.Count > 0 ? Math.Round(monthlyVotes.Average(v => v.Rate), 2) : 0,
                MonthlyTotalVotes  = monthlyVotes.Count,
                MonthlyPeriodStart = monthlyPeriodStart,
                MonthlyPeriodEnd   = monthlyPeriodEnd,
            };
        })
        .OrderByDescending(r => r.MonthlyAverageRate)
        .ThenByDescending(r => r.MonthlyTotalVotes)
        .ToList();

    return new Response.RankingResponse
    {
        WeeklyRanking  = weeklyRanking,
        MonthlyRanking = monthlyRanking,
    };
}


    private Guid GetUserCurrentId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("You must log in");
        }

        return Guid.Parse(userId);
    }
}