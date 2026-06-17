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
        public string? ChapterFileUrl { get; set; }
        public ChapterStatus Status { get; set; }
        public Guid SeriesId { get; set; }
        public string SeriesTitle { get; set; } = string.Empty;
        public DateTimeOffset Deadline { get; set; }
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
    
    public class GetChapterDetailsResponse
    {
        public Guid ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public required string Title { get; set; }
        public string? Summary { get; set; }
        public string? ManuscriptFileUrl { get; set; }
        public string? ChapterFileUrl { get; set; }
        public ChapterStatus Status { get; set; }
        public Guid SeriesId { get; set; }
        public string SeriesTitle { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public List<TaskSummary> Tasks { get; set; } = new();
    }
    
    public class TaskSummary
    {
        public Guid MangaTaskId { get; set; }
        public required string TaskTitle { get; set; }
        public string? TaskDescription { get; set; }
        public MangaTaskStatus Status { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
    }
    
    public class UpdateChapterResponse
    {
        public Guid ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public required string Title { get; set; }
        public string? Summary { get; set; }
        public string? ManuscriptFileUrl { get; set; }
        public string? ChapterFileUrl { get; set; }  
        public ChapterStatus Status { get; set; }
        public Guid SeriesId { get; set; }
        public string SeriesTitle { get; set; } = string.Empty;
        public string UpdatedByName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public List<TaskSummary> Tasks { get; set; } = new();
    }
}