using Manga.Repository.Entity.Enums;

namespace Manga.Service.UserProfile;

public interface IService
{
    public Task<Response.GetProfileResponse> GetProfile();
    public Task<Response.GetProfileResponse> UpdateProfile(Request.UpdateProfileRequest request);
    public Task<List<Response.GetUserListResponse>> GetUserList();
    public Task<List<Response.GetUserListByRoleResponse>> GetUserListByRole(Request.GetUserListByRoleRequest request);
    public Task<Response.GetProfileResponse> UpdateUserStatus(Request.UpdateUserStatusRequest request);
}