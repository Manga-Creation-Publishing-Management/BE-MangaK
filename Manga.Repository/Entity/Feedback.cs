using Manga.Repository.Abtraction;

namespace Manga.Repository.Entity;

public class Feedback: BaseEntity<Guid>, IAuditableEntity
{
    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null;
    public Guid ReceiverId { get; set; }
    public User Receiver { get; set; } = null;
    public required string Content { get; set; }
    
    
    public Guid? SeriesId { get; set; }
    public Series? Series { get; set; }

    public Guid? ChapterId { get; set; }
    public Chapter? Chapter { get; set; }

    public Guid? MangaTaskId { get; set; }
    public MangaTask? MangaTask { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}