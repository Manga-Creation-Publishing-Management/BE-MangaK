namespace Manga.Service.Auth;

public interface IService
{
    Task<Response.LoginResponse> Login(Request.LoginRequest request);
    Task<Response.RegistrationResponse> Register(Request.RegisterRequest request);
    Task<Response.RefreshTokenResponse>  RefreshToken(Request.RefreshTokenRequest request);
    Task<Response.ForgotPasswordResponse>  ForgotPassword(Request.ForgotPasswordRequest request);
    Task<Response.ChangePasswordResponse> ChangePassword(Request.ChangePasswordRequest request);
    
    
}