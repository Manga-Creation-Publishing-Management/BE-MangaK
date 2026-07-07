using Manga.Repository.Abtraction;

namespace Manga.Repository.Entity;

public class UserSession : BaseEntity<Guid>, IAuditableEntity
{
    public User? User { get; set; }
    public  Guid? UserId { get; set; }
    public Reader? Reader { get; set; }
    public  Guid? ReaderId { get; set; }
    public required string DeviceFingerprint{get;set;}
    public required string RefreshToken{get;set;}
    public required DateTimeOffset ExpiresAt{get;set;}
    public required bool IsRevoked{get;set;}

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}