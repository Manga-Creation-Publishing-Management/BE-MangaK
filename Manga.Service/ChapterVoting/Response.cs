namespace Manga.Service.ChapterVoting;

public class Response
{
    public class VoteChapterResponse
    {
        public Guid Id { get; set; }
        public Guid ChapterId { get; set; }
        public int Rate { get; set; }
        public DateTimeOffset VoteAt { get; set; }
    }
    
    public class WeeklyRankingResponse
    {
        public Guid SeriesId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverFile { get; set; }
        public double WeeklyAverageRate { get; set; }
        public int WeeklyTotalVotes { get; set; }
        public DateTimeOffset WeeklyPeriodStart { get; set; }
        public DateTimeOffset WeeklyPeriodEnd { get; set; }
    }

    public class MonthlyRankingResponse
    {
        public Guid SeriesId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverFile { get; set; }
        public double MonthlyAverageRate { get; set; }
        public int MonthlyTotalVotes { get; set; }
        public DateTimeOffset MonthlyPeriodStart { get; set; }
        public DateTimeOffset MonthlyPeriodEnd { get; set; }
    }

    public class RankingResponse
    {
        public List<WeeklyRankingResponse> WeeklyRanking { get; set; } = new();
        public List<MonthlyRankingResponse> MonthlyRanking { get; set; } = new();
    }
    
    public class GetReaderVoteResponse
    {
        public Guid ReaderId { get; set; }
        public Guid ChapterId { get; set; }
        public int Rating { get; set; }
    }
}