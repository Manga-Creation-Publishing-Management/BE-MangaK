using Manga.Repository.Entity.Enums;

namespace Manga.Service.PublishingSchedule;

public class Response
{
    public class CreatePublishingScheduleResponse
    {
        public Guid ScheduleId { get; set; }
        public Guid SeriesId { get; set; }
        public required string SeriesTitle { get; set; }
        public SeriesStatus SeriesStatus { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public string? PublishPeriod { get; set; }
        public string DecidedByName { get; set; } = string.Empty;
        public DateTimeOffset CreateAt { get; set; }
    }
    
    public class GetPublishingScheduleResponse
    {
        public Guid ScheduleId { get; set; }
        public Guid SeriesId { get; set; }
        public required string SeriesTitle { get; set; }
        public string? SeriesCoverFile { get; set; }
        public SeriesStatus SeriesStatus { get; set; }
        public string MangakaName { get; set; } = string.Empty;
        public DateTimeOffset PublishDate { get; set; }
        public string? PublishPeriod { get; set; }
        public string DecidedByName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
    
}