namespace Manga.Service.PublishingSchedule;

public class Request
{
    public class CreatePublishingScheduleRequest
    {
        public required DateTimeOffset PublishDate { get; set; }
        public string? PublishPeriod { get; set; }
    }
    public class UpdatePublishingScheduleRequest
    {
        public required DateTimeOffset PublishDate { get; set; }
        public string? PublishPeriod { get; set; }
    }
}