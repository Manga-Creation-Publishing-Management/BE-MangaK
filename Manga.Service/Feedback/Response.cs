using Manga.Repository.Entity.Enums;

namespace Manga.Service.Feedback;

public class Response
{
    public class GetFeedBackDetailResponse
    {
        public Guid Id { get; set; }
        public required Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public Guid? SeriesId { get; set; }
        public Guid? ChapterId { get; set; }
        public Guid? MangaTaskId { get; set; }
        public string? SeriesTitle { get; set; }
        public string? ChapterTitle { get; set; }
        public string? MangaTaskTitle { get; set; }
        public required string Content { get; set; }
        public FeedbackType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}