using Manga.Repository.Abtraction;
using Manga.Repository.Entity.Enums;

namespace Manga.Repository.Entity;

public class ChapterVoting: BaseEntity<Guid>, IAuditableEntity
{
    public int Rate { get; set; }
    public DateTimeOffset VoteAt { get; set; }
    public VoteStatus Status { get; set; }
    
    public Guid ReaderId { get; set; }
    public User Reader { get; set; } = null;
    
    public Guid ChapterId { get; set; }
    public Chapter Chapter { get; set; } = null;
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}