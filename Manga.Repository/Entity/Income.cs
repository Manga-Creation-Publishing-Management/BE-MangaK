using Manga.Repository.Abtraction;
using Manga.Repository.Entity.Enums;

namespace Manga.Repository.Entity;

public class Income: BaseEntity<Guid>, IAuditableEntity
{
    public required decimal Amount { get; set; }
    public DateTimeOffset? Date { get; set; }
    public IncomeStatus Status { get; set; }
    
    public Guid MangaTaskId { get; set; }
    public MangaTask MangaTask { get; set; } = null!;
    
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}