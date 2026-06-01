using Manga.Repository.Abtraction;
using Manga.Repository.Entity.Enums;

namespace Manga.Repository.Entity;

public class Series: BaseEntity<Guid>, IAuditableEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? CoverFile { get; set; }
    // public string? CoverFilePublicId { get; set; } 
    public string? NameFile { get; set; }
    public string? NameFilePublicId { get; set; } 
    public SeriesStatus Status { get; set; }
    
    public Guid CreatedById  { get; set; }
    public User CreatedBy  { get; set; } = null!;
    
    public Guid? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }
    
    public Guid? ReviewedById  { get; set; }
    public User? ReviewedBy { get; set; }
    
    public PublishingSchedule? PublishingSchedule { get; set; }
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();
    public ICollection<CategorySeries> CategorySeries { get; set; } = new List<CategorySeries>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}