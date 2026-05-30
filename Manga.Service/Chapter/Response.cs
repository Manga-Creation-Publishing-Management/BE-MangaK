using Manga.Repository.Entity.Enums;

namespace Manga.Service.Chapter;

public class Response
{
    public class CreateChapterResponse
    {
        public Guid ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public required string Title { get; set; }
        public string? Summary { get; set; }
        public string? ManuscriptFileUrl { get; set; }
        public ChapterStatus Status { get; set; }
        public Guid SeriesId { get; set; }
        public string SeriesTitle { get; set; } = string.Empty;
        public DateTimeOffset CreateAt { get; set; }
    }
    
    public class GetAllChaptersResponse
    {
        public Guid ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public required string Title { get; set; }
        public string? Summary { get; set; }
        public ChapterStatus Status { get; set; }
        public int TotalTask { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    
}