using Manga.Repository.Abtraction;
using Manga.Repository.Entity.Enums;

namespace Manga.Repository.Entity;

public class Chapter: BaseEntity<Guid>, IAuditableEntity
{
    public required int ChapterNumber { get; set; }
    public required string Title { get; set; }
    public string? Summary { get; set; }
    public string? ManuscriptFileUrl { get; set; }
    public string? ChapterFileUrl  { get; set; }
    public DateTimeOffset Deadline { get; set; }
    public ChapterStatus Status { get; set; }
    
    public Guid SeriesId { get; set; }
    public Series Series { get; set; } = null;
    
    public ICollection<MangaTask> MangaTasks { get; set; } = new List<MangaTask>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<ChapterVoting> ChapterVotes { get; set; } = new List<ChapterVoting>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}