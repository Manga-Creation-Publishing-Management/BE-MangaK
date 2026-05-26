using Manga.Repository.Abtraction;

namespace Manga.Repository.Entity;

public class Mangaka: BaseEntity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}