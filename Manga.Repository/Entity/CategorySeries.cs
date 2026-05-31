namespace Manga.Repository.Entity;

public class CategorySeries
{
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid SeriesId { get; set; }
    public Series Series { get; set; } = null!;
}