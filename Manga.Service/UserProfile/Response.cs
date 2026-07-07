using Manga.Repository.Entity.Enums;

namespace Manga.Service.UserProfile;

public class Response
{
    public class GetProfileResponse
    {
     public required Guid Id { get; set; }
     public required string FirstName { get; set; }
     public required string LastName { get; set; }
     public required string Email { get; set; }
     public string? AvatarUrl { get; set; }
     public string? Bio { get; set; }
     public string? AuthorName { get; set; }
     public string? Phone { get; set; }
     public Guid? SupervisorId { get; set; }
    }
    public class GetUserListByRoleResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class GetUserListResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public Guid? SupervisorId { get; set; }
    }
}