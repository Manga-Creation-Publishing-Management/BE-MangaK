using System.Security.Claims;
using Manga.Repository.Data;
using Manga.Repository.Entity;
using Manga.Service.MailService;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.Auth;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService.IService _jwtService;
    private readonly MailService.IService _mailService;

    public Service(AppDbContext dbContext, JwtService.IService jwtService, MailService.IService mailService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _mailService = mailService;
    }

    public async Task<Response.LoginResponse> Login(Request.LoginRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.Verified) // dòng này kiểm tra xem user này đã xác thực email chưa kiểu dữ liệu là bool
            throw new InvalidOperationException("Account is not verified.");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var claim = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var accessToken = _jwtService.GenerateAccessToken(claim);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            DeviceFingerprint = request.DeviceFingerprint ?? "Unknown",
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync();

        return new Response.LoginResponse()
        {
            UserId = user.Id,
            Email = user.Email,
            // FullName = $"{user.FirstName} {user.LastName}",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            // Phone =  user.Phone,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<Response.RegistrationResponse> Register(Request.RegisterRequest request)
    {
        var emailExist = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExist)
        {
            throw new ArgumentException("Email already exists");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required");
        }

        if (request.Password.Length < 6)
        {
            throw new ArgumentException("Password is too short.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone ?? "",
            Role = request.Role,
            Verified = true,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Add(user);
        await _dbContext.SaveChangesAsync();
        return new Response.RegistrationResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString()
        };
    }

    public Task<Response.RegisterReaderResponse> RegisterReader(Request.RegisterReaderRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<Response.LoginResponse> RefreshToken(Request.RefreshTokenRequest request)
    {
        var session =
            await _dbContext.UserSessions.FirstOrDefaultAsync(x =>
                x.RefreshToken == request.RefreshToken && !x.IsRevoked)
            ?? throw new UnauthorizedAccessException("Invalid refresh token or session has expired");
        if (session.ExpiresAt < DateTime.UtcNow)
        {
            session.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
            throw new UnauthorizedAccessException("Your session has expired. Please log in again.");
        }

        if (request.DeviceFingerprint != session.DeviceFingerprint)
        {
            session.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
            throw new UnauthorizedAccessException("Access denied. Your session is invalid or has expired.");
        }

        var user = session.User;
        session.IsRevoked = true;

        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            DeviceFingerprint = request.DeviceFingerprint ?? "Unknown",
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.UserSessions.AddAsync(newSession);
        await _dbContext.SaveChangesAsync();
        var claim = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var accessToken = _jwtService.GenerateAccessToken(claim);
        return new Response.LoginResponse()
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
        };
    }

    public async Task<String> ForgotPassword(Request.ForgotPasswordRequest request)
    {
        var user = _dbContext.Users.FirstOrDefault(x => x.Email == request.Email);
        if (user == null)
        {
            throw new ArgumentException("Email does not used");
        }

        if (user.Verified == false)
        {
            throw new ArgumentException("User has not been verified");
        }

        var resetCode = new Random().Next(100000, 999999);
        user.ResetPasswordCode = resetCode;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        await _mailService.SendMail(new MailContent()
        {
            To = request.Email,
            Subject = "Mangaka - Forgot Password",
            Body = BuildVerificationEmailBody($"{user.FirstName} {user.LastName}", resetCode)
        });

        return "Please check email";
    }

    public async Task<String> ChangePassword(Request.ChangePasswordRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(s => request.Code == s.ResetPasswordCode);
        if (user == null) throw new ArgumentException("Invalid code");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetPasswordCode = 0;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return "Change Password successfully. Please login again.";
    }

    public async Task<bool> Logout(Request.LogoutRequest request, CancellationToken cancellationToken)
    {
        var session = await _dbContext.UserSessions.FirstOrDefaultAsync(x =>
            x.RefreshToken == request.refreshToken && !x.IsRevoked && !x.IsDeleted &&
            x.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);

        if (session == null)
        {
            return false;
        }

        session.IsRevoked = true;
        session.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string BuildVerificationEmailBody(string fullName, int verifiedCode) => $"""
         <!DOCTYPE html>
         <html lang="vi">
         <head>
             <meta charset="UTF-8" />
             <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
             <title>Mangaka Verification</title>
         </head>

         <body style="
             margin:0;
             padding:0;
             background:#f0e6ff;
             font-family:Arial,Helvetica,sans-serif;
         ">

         <table width="100%" cellpadding="0" cellspacing="0" border="0">
         <tr>
         <td align="center" style="padding:40px 16px;">

             <!-- Container -->
             <table width="600" cellpadding="0" cellspacing="0" border="0"
                    style="
                         background:#ffffff;
                         border-radius:28px;
                         overflow:hidden;
                         box-shadow:0 12px 40px rgba(186,104,200,0.15);
                    ">

                 <!-- HEADER -->
                 <tr>
                     <td style="
                         background:linear-gradient(135deg, #E6E0F8 0%, #E1D6E2 60%, #B8A8E6 100%);
                         padding:30px 32px;
                         text-align:center;
                         position:relative;
                     ">

                         <!-- Floating Circle -->
                         <div style=" width:300px; height:72px; margin:0 auto 0px; border-radius:20px; background:rgba(255, 183, 104, 0.25); line-height:72px; text-align:center; font-size:28px; font-weight:800; color:rgb(255, 255, 255); border:1px solid rgba(255,255,255,0.2); "> Mangaka </div>

                         <p style="
                             margin:16px 0 0;
                             color:#b0a0e0;
                             font-size:16px;
                             line-height:1.7;
                         ">
                             Thế giới truyện tranh trong tầm tay bạn
                         </p>

                     </td>
                 </tr>

                 <!-- BODY -->
                 <tr>
                     <td style="padding:50px 40px; color:#311b92;">

                         <p style="
                             margin:0 0 18px;
                             font-size:26px;
                             font-weight:700;
                             color:#1a0033;
                         ">
                             Xin chào <strong style="color:#b39ddb;">{fullName}</strong>,
                         </p>

                         <p style="
                             margin:0 0 18px;
                             font-size:16px;
                             line-height:1.9;
                             color:#5e35b1;
                         ">
                             Cảm ơn bạn đã đăng ký tài khoản tại
                             <strong style="color:#b39ddb;">Mangaka</strong>.
                         </p>

                         <p style="
                             margin:0 0 36px;
                             font-size:16px;
                             line-height:1.9;
                             color:#5e35b1;
                         ">
                             Vui lòng sử dụng mã xác thực bên dưới để hoàn tất quá trình xác thực tài khoản của bạn.
                         </p>

                         <!-- OTP CARD -->
                         <div style="
                             background:linear-gradient(135deg, #ffd180 0%, #b39ddb 100%);
                             border-radius:24px;
                             padding:40px 20px;
                             text-align:center;
                             margin:40px 0;
                             box-shadow:0 12px 30px rgba(255,183,104,0.25);
                             position:relative;
                             overflow:hidden;
                         ">

                             <!-- Glow -->
                             <div style="
                                 position:absolute;
                                 width:200px;
                                 height:200px;
                                 background:rgba(230, 224, 248, 0.1);
                                 border-radius:50%;
                                 top:-80px;
                                 right:-60px;
                             "></div>

                             <p style="
                                 margin:0 0 16px;
                                 color:#ffffff;
                                 font-size:14px;
                                 letter-spacing:2px;
                                 text-transform:uppercase;
                             ">
                                 MÃ XÁC THỰC (OTP)
                             </p>

                             <div style="
                                 display:inline-block;
                                 background:#ffffff;
                                 padding:18px 34px;
                                 border-radius:18px;
                                 box-shadow:0 8px 24px rgba(0,0,0,0.15);
                             ">
                                 <span style="
                                     font-size:36px;
                                     font-weight:800;
                                     letter-spacing:12px;
                                     color:#ffb74d;
                                 ">
                                     {verifiedCode}
                                 </span>
                             </div>

                             <p style="
                                 margin:18px 0 0;
                                 color:#ffffff;
                                 font-size:13px;
                             ">
                                 Mã có hiệu lực trong 5 phút
                             </p>

                         </div>

                         <!-- BUTTON -->
                         <div style="text-align:center; margin:42px 0;">

                             <a href="#"
                                style="
                                     display:inline-block;
                                     background:#b39ddb;
                                     color:#ffffff;
                                     text-decoration:none;
                                     padding:16px 34px;
                                     border-radius:16px;
                                     font-size:15px;
                                     font-weight:700;
                                     box-shadow:0 10px 24px rgba(179,157,219,0.35);
                                ">
                                 Xác thực tài khoản
                             </a>

                         </div>

                         <!-- INFO BOX -->
                         <div style="
                             background:#f8f0ff;
                             border:1px solid #e1bee7;
                             border-left:5px solid #ffb74d;
                             border-radius:16px;
                             padding:18px 20px;
                         ">

                             <p style="
                                 margin:0;
                                 color:#7e57c2;
                                 font-size:14px;
                                 line-height:1.8;
                             ">
                                 Nếu bạn không yêu cầu tạo tài khoản,
                                 hãy bỏ qua email này để đảm bảo an toàn.
                             </p>

                         </div>

                     </td>
                 </tr>

                 <!-- FOOTER -->
                 <tr>
                     <td style="
                         background:#7e57c2;
                         padding:34px 24px;
                         text-align:center;
                     ">

                         <h3 style="
                             margin:0 0 10px;
                             color:#ffb74d;
                             font-size:20px;
                         ">
                             Mangaka
                         </h3>

                         <p style="
                             margin:0 0 16px;
                             color:#ffffff;
                             font-size:14px;
                             line-height:1.7;
                         ">
                             Nền tảng đọc truyện tranh trực tuyến hàng đầu.
                         </p>

                         <p style="
                             margin:0;
                             color:#e6e0f8;
                             font-size:12px;
                         ">
                             © 2026 Mangaka. All rights reserved.
                         </p>

                     </td>
                 </tr>

             </table>

         </td>
         </tr>
         </table>

         </body>
         </html>
         """;
}