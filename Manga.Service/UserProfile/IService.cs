namespace Manga.Service.UserProfile;

public interface IService
{
    public Task<Response.GetProfileResponse> GetProfile();
    public Task<Response.GetProfileResponse> UpdateProfile(Request.UpdateProfileRequest request);
    public Task<List<Response.GetUserListByRole>> GetUserListByRole(Request.GetUserListByRoleRequest request);
    
}