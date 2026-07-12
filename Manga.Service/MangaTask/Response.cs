using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;

namespace Manga.Service.MangaTask;

public class Response
{
    public class CreateNewTaskResponse
    {
        public Guid Id { get; set; }
        public required string TaskTitle { get; set; }
        public string? TaskDescription { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public DateTimeOffset? AssignedAt { get; set; }
        public Guid ChapterId { get; set; }
        public Guid AssignedToId { get; set; }
        public decimal Income { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid SeriesId { get; set; }
    }

    public class GetTaskDetailsResponse
    {
        public Guid Id { get; set; }
        public string SeriesTitle { get; set; }
        public required Guid ChapterId { get; set; }
        
        public string ChapterTitle { get; set; }
        public int ChapterNumber { get; set; }
        public int? TotalPages { get; set; }
        public string ManuscriptFileUrl { get; set; }
        
        public required string TaskTitle { get; set; }
        public string? TaskDescription { get; set; }
        public string? SubmittedFileUrl { get; set; }
        public MangaTaskStatus Status { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public DateTimeOffset? AssignedAt { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public Guid CreatedById { get; set; }
        public Guid AssignedToId { get; set; }
        public string AssistantName { get; set; }

        public string MangakaAuthorName { get; set; }
        public decimal? IncomeAmount { get; set; }
        public List<FeedbackSummaryResponse>? Feedback { get; set; } = new();
    }

    public class FeedbackSummaryResponse
    {
        public Guid FeedbackId { get; set; }
        public Guid SenderId { set; get; }
        public required string Content { set; get; }
        public required DateTimeOffset CreatedAt { set; get; }
    }
    public class GetTotalTaskResponse
    {
        public int Total { get; set; }
        public int NumberOfStatus { get; set; }
    }

    public class GetPageRangeResponse
    {
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public MangaTaskStatus Status { get; set; }
        public Guid AssignedToId { get; set; }
        public string AssistantName { get; set; }
    }
}