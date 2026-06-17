using Microsoft.AspNetCore.Http;

namespace Manga.Service.Series;

public class Request
{
    public class CreateSeriesRequest
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required List<Guid> CategoryIds { get; set; } 
        public IFormFile? CoverFile { get; set; }
        public IFormFile? NameFile { get; set; }
    }
    
    public class ReviewSeriesRequest
    {
        public required bool IsApproved { get; set; }
        public string? Note { get; set; }
    }
    
    public class GetSeriesByCategoryRequest
    {
        public required List<Guid> CategoryIds { get; set; }
    }
    
    public class CancelSeriesRequest
    {
        public required string Reason { get; set; }
    }

    public class SearchSeriesByVotingRequest
    {
        public double MinRate { get; set; } = 0;
        public double MaxRate { get; set; } = 5;
        public string RankingType { get; set; } = "Weekly";
    }
}