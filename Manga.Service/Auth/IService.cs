using Manga.Repository.Entity.Enums;

namespace Manga.Service.Auth;

public interface IService
{
    Task<Response.LoginResponse> Login(Request.LoginRequest request);
    Task<Response.RegistrationResponse> Register(Request.RegisterRequest request,UserRole role);
    Task<Auth.Response.LoginByGoogleResponse> LoginByGoogle(Request.LoginByGoogleRequest request);
    Task<Response.LoginResponse>  RefreshToken(Request.RefreshTokenRequest request);
    Task<String>  ForgotPassword(Request.ForgotPasswordRequest request);
    Task<String> ChangePassword(Request.ChangePasswordRequest request);
    Task<bool> Logout(Request.LogoutRequest request, CancellationToken cancellationToken);
}