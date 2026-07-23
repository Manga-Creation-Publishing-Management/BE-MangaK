using Manga.Repository.Abtraction;
using Manga.Repository.Entity.Enums;

namespace Manga.Repository.Entity;

public class Leaderboard: BaseEntity<Guid>, IAuditableEntity
{
    public DateTime PeriodStart { get; set; }//
    public DateTime PeriodEnd { get; set; }//
    public required int RankPosition { get; set; }
    public int TotalVotes { get; set; }
    public double AverageRate { get; set; }//
    
    public Guid SeriesId { get; set; }
    public Series Series { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}