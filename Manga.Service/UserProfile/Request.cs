using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;

namespace Manga.Service.UserProfile;

public class Request
{
    public class GetUserListByRoleRequest
    {
        public UserRole UserRole { get; set; }
    }
    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public string? Bio { get; set; }
        public string? AuthorName { get; set; }
    }
    public class UpdateUserStatusRequest
    {
        public Guid? UserId { get; set; }
        public UserStatus Status { get; set; }
    }
    public class FilterAssistantRequest
    {
        public Guid? MangaId { get; set; }
        public Guid? ChapterId { get; set; }
    }
}