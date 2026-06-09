using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;

namespace Manga.Service.MangaTask;

public class Request
{
    public class CreateNewTaskRequest
    {
        public required string TaskTitle { get; set; }
        public string? TaskDescription { get; set; }
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
}