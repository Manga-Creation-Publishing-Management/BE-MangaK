using Manga.Repository.Entity.Enums;

namespace Manga.Service.Feedback;

public class Request
{
    public class SendFeedbackRequest
    {
        public Guid? SeriesId { get; set; }
        public Guid? ChapterId { get; set; }
        public Guid? MangaTaskId { get; set; }
        public required string Content { get; set; }
        public FeedbackType Type { get; set; } = FeedbackType.Manual;
    }
    public class GetFeedBackRequest
    {
        public Guid SeriesId { get; set; }
        public Guid? ChapterId { get; set; }
        public Guid? MangaTaskId { get; set; }
    }
}