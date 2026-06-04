using Manga.Repository.Abtraction;

namespace Manga.Repository.Entity;

public class PublishingSchedule: BaseEntity<Guid>, IAuditableEntity
{
    public DateTimeOffset PublishDate { get; set; }
    public string? PublishPeriod { get; set; }
    
    public Guid SeriesId { get; set; }
    public Series Series { get; set; } = null!;
    
    public Guid? DecidedById  { get; set; }
    public User? DecidedBy  { get; set; } = null; // chỗ này là lịch được quyết định bởi 1 user có role là EditorialBoard nhé
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}