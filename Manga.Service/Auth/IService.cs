namespace Manga.Service.Auth;

public interface IService
{
    Task<Response.LoginResponse> Login(Request.LoginRequest request);
    Task<Response.RegistrationResponse> Register(Request.RegisterRequest request);
    Task<Auth.Response.RegisterReaderResponse> RegisterReader(Request.RegisterReaderRequest request);
    Task<Response.LoginResponse>  RefreshToken(Request.RefreshTokenRequest request);
    Task<Response.ForgotPasswordResponse>  ForgotPassword(Request.ForgotPasswordRequest request);
    Task<Response.ChangePasswordResponse> ChangePassword(Request.ChangePasswordRequest request);
    Task<bool> Logout(Request.LogoutRequest request, CancellationToken cancellationToken);

}