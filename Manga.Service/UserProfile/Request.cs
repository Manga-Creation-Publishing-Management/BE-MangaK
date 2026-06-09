using Manga.Repository.Entity.Enums;

namespace Manga.Service.UserProfile;

public class Request
{
    public class GetUserListByRoleRequest
    {
        public UserRole UserRole { get; set; }
    }
}