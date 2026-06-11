using Manga.Repository.Abtraction;

namespace Manga.Repository.Entity;

public class Reader : BaseEntity<Guid>, IAuditableEntity
{
    public required string Email { get; set; }
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? GoogleAccountId { get; set; }

    public ICollection<ChapterVoting> ChapterVotings { get; set; } = new List<ChapterVoting>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
