using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;

namespace Manga.Service.MangaTask;

public class Request
{
    public class CreateNewTaskRequest
    {
        public required Guid SeriesId { get; set; }
        public required string TaskTitle { get; set; }
        public required int From { get; set; }
        public required int To { get; set; }
        
        public DateTimeOffset Deadline { get; set; }

        public Guid ChapterId { get; set; }
        public Guid AssignedToId { get; set; }
        public decimal AmountIncome { get; set; }
    }

    public class GetTaskDetailsRequest
    {
        public Guid TaskId { get; set; }
    }

    public class GetTaskListRequest
    {
        public Guid? ChapterId { get; set; }
        public MangaTaskStatus? Status { get; set; }
    }

    public class UpdateTaskStatusRequest
    {
        public Guid TaskId { get; set; }
        public MangaTaskStatus Status { get; set; }
    }
    
        public class SubmitTaskRequest
    {
        public Guid TaskId { get; set; }
        public required IFormFile SubmittedFileUrl { get; set; }
    }

    public class ReviewTaskRequest
    {
        public Guid TaskId { get; set; }
        public bool IsApproved { get; set; }
        public string? FeedbackContent { get; set; }
    }

    public class UpdateMangaTaskRequest
    {
        public Guid TaskId { get; set; }
        public DateTimeOffset Deadline { get; set; }
    }

    public class ReassignTaskRequest
    {
        public Guid TaskId { get; set; }
        public Guid NewAssistantId { get; set; }
    }
}