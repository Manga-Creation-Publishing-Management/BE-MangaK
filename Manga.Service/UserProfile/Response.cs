namespace Manga.Service.UserProfile;

public class Response
{
    public class GetProfileResponse
    {
        
    }
    public class GetUserListByRole
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
    public class GetAssistantListResponse
    {
        public required Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
    }
}