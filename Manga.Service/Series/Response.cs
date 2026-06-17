using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;

namespace Manga.Service.Series;

public class Response
{
    public class CreateSeriesResponse
    {
        public Guid SeriesId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public List<string> Categories { get; set; } = new(); 
        public string? CoverFile { get; set; }
        public string? NameFile { get; set; }
        public string? NameFilePublicId { get; set; } 
        public SeriesStatus Status { get; set; }
        public DateTimeOffset CreateAt { get; set; }
    }
    
    public class GetAllSeriesResponse
    {
        public Guid SeriesId { get; set; }
        public required string Title { get; set; }
        public string MangakaName { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new(); 
        public string? CoverFile { get; set; }
        public int TotalChapters { get; set; }
        public SeriesStatus Status { get; set; }
        public DateTimeOffset CreateAt { get; set; }
    }
    
    public class GetSeriesDetailsResponse
    {
       public Guid SeriesId { get; set; }
       public required string Title { get; set; }
       public required string Description { get; set; }
       public List<string> Categories { get; set; } = new(); 
       public string? CoverFile { get; set; }
       public string? NameFile { get; set; }
       public SeriesStatus Status { get; set; }
       public string MangakaName { get; set; } = string.Empty;
       public DateTimeOffset? PublishDate { get; set; }
       public string? PublishPeriod { get; set; }
       public DateTimeOffset CreateAt { get; set; }
       public List<ChapterSummary> Chapters { get; set; } = new();
    }
    
    public class ChapterSummary
    {
       public Guid ChapterId { get; set; }
       public int ChapterNumber { get; set; }
       public required string Title { get; set; }
       public string? Summary { get; set; }
       public ChapterStatus Status { get; set; }
       public DateTimeOffset CreatedAt { get; set; }
    }
    
    public class ReviewSeriesResponse
    {
        public Guid SeriesId { get; set; }
        public required string Title { get; set; }
        public SeriesStatus Status { get; set; }
        public string? Note { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerRole { get; set; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; set; }
        
    }
    
    public class CancelSeriesResponse
    {
        public Guid SeriesId { get; set; }
        public required string Title { get; set; }
        public SeriesStatus Status { get; set; }
        public required string Reason { get; set; }
        public string CancelledByName { get; set; } = string.Empty;
        public DateTimeOffset CancelledAt { get; set; }
    }

    public class SearchSeriesByVotingResponse
    {
        public List<ChapterVoting.Response.WeeklyRankingResponse> WeeklyRanking { get; set; } = new();
        public List<ChapterVoting.Response.MonthlyRankingResponse> MonthlyRanking { get; set; } = new();
    }
}