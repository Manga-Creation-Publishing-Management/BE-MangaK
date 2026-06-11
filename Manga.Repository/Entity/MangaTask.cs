using Manga.Repository.Abtraction;
using Manga.Repository.Entity.Enums;

namespace Manga.Repository.Entity;

public class MangaTask: BaseEntity<Guid>, IAuditableEntity
{
    public required string TaskTitle { get; set; }
    public string? TaskDescription { get; set; }
    public string? submittedFileUrl  { get; set; }
    public MangaTaskStatus Status { get; set; }
    public DateTimeOffset Deadline { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    
    public Guid ChapterId { get; set; }
    public Chapter Chapter { get; set; } = null;
    
    public Guid CreatedById  { get; set; }
    public User CreatedBy  { get; set; } = null;
    
    public Guid AssignedToId  { get; set; }
    public User AssignedTo  { get; set; } = null;
    
    public Income? Income { get; set; }
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}