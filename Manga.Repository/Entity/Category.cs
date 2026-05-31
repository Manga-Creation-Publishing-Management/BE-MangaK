using Manga.Repository.Abtraction;

namespace Manga.Repository.Entity;
public class Category : BaseEntity<Guid>
{
    public required string Name { get; set; }

    public ICollection<CategorySeries> CategorySeries { get; set; } = new List<CategorySeries>();
}