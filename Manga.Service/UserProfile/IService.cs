namespace Manga.Service.UserProfile;

public interface IService
{
    public Task<Response.GetProfileResponse> GetProfile();
    public Task<Response.GetProfileResponse> UpdateProfile(Request.UpdateProfileRequest request);
    public Task<List<Response.GetUserListResponse>> GetUserList();
    public Task<List<Response.GetUserListByRoleResponse>> GetUserListByRole(Request.GetUserListByRoleRequest request);
    
}