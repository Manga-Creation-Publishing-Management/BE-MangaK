using System.ComponentModel.DataAnnotations;
using Manga.Repository.Abtraction;
using Manga.Repository.Entity.Enums;

namespace Manga.Repository.Entity;

public class User: BaseEntity<Guid>, IAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Phone  { get; set; }
    public string? AvatarUrl { get; set; }

    public string? AuthorName { get; set; }
    public string? Bio { get; set; }
    
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    
    public bool Verified { get; set; }
    public int VerifiedCode {get; set;}
    public int ResetPasswordCode {get; set;}
    public Guid? SupervisorId { get; set; }
    public User? Supervisor { get; set; }
    public ICollection<Feedback>  SendFeedbacks { get; set; } = new List<Feedback>();
    //magaka
    public ICollection<Series> CreatedSeries { get; set; } = new List<Series>();
    public ICollection<MangaTask> CreatedTasks { get; set; } = new List<MangaTask>();
    //editorial board
    public ICollection<Series> ApprovedSeries { get; set; } = new List<Series>();
    public ICollection<PublishingSchedule> DecidedSchedules { get; set; } = new List<PublishingSchedule>();
    //assistant
    public ICollection<MangaTask> AssignedTasks { get; set; } = new List<MangaTask>();

    //tantou editer
    public ICollection<Series> ReviewedSeries { get; set; } = new List<Series>();
    public ICollection<User> Mangakas { get; set; } = new List<User>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}