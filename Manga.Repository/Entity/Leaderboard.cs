using Manga.Repository.Abtraction;

namespace Manga.Repository.Entity;

public class Leaderboard: BaseEntity<Guid>, IAuditableEntity
{
    public required string RankingPeriod { get; set; }
    public required int RankPosition { get; set; }
    public decimal TotalVotes { get; set; }
    
    public Guid SeriesId { get; set; }
    public Series Series { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}