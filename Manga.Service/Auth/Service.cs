using System.Security.Claims;
using Manga.Repository.Data;
using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
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

    public async Task<Response.RegistrationResponse> Register(Request.RegisterRequest request, UserRole role)
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
            Role = role,
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
         <html lang="en">
         <head>
             <meta charset="UTF-8" />
             <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
             <title>Mangaka Verification</title>
         </head>

         <body style="
             margin:0;
             padding:0;
             background:#F4F0FA;
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
                         box-shadow:0 12px 40px rgba(142, 68, 173, 0.1);
                    ">

                 <!-- HEADER -->
                 <tr>
                     <td style="
                         background:linear-gradient(135deg, #E8E5F7 0%, #F5EFFB 100%);
                         padding:40px 32px;
                         text-align:center;
                         position:relative;
                     ">

                         <!-- Brand Header -->
                         <div style=" 
                            width:280px; 
                            height:64px; 
                            margin:0 auto; 
                            border-radius:20px; 
                            background:#FFF2E6; 
                            line-height:64px; 
                            text-align:center; 
                            font-size:26px; 
                            font-weight:800; 
                            color:#FF9F43; 
                            border:1px solid #FFE0C2;
                         ">Mangaka</div>

                         <p style="
                             margin:16px 0 0;
                             color:#9B8BB4;
                             font-size:15px;
                             font-weight: 500;
                             letter-spacing: 0.5px;
                         ">
                             Your ultimate manga universe at your fingertips
                         </p>

                     </td>
                 </tr>

                 <!-- BODY -->
                 <tr>
                     <td style="padding:40px 40px 30px; color:#4A3F60;">

                         <p style="
                             margin:0 0 18px;
                             font-size:24px;
                             font-weight:700;
                             color:#2D253B;
                         ">
                             Hi <strong style="color:#FF9F43;">{fullName}</strong>,
                         </p>

                         <p style="
                             margin:0 0 14px;
                             font-size:15px;
                             line-height:1.7;
                             color:#6C5E85;
                         ">
                             Thank you for registering an account with <strong style="color:#9B5DE5;">Mangaka</strong>.
                         </p>

                         <p style="
                             margin:0 0 30px;
                             font-size:15px;
                             line-height:1.7;
                             color:#6C5E85;
                         ">
                             Please use the verification code below to complete your account setup.
                         </p>

                         <!-- OTP CARD -->
                         <div style="
                             background:linear-gradient(135deg, #F3EAFE 0%, #FFF0E0 100%);
                             border-radius:24px;
                             padding:36px 20px;
                             text-align:center;
                             margin:30px 0;
                             border: 1px solid #EBE2F7;
                             position:relative;
                             overflow:hidden;
                         ">

                             <!-- Glow -->
                             <div style="
                                 position:absolute;
                                 width:200px;
                                 height:200px;
                                 background:rgba(255,255,255,0.6);
                                 border-radius:50%;
                                 top:-80px;
                                 right:-60px;
                             "></div>

                             <p style="
                                 margin:0 0 16px;
                                 color:#9B8BB4;
                                 font-size:13px;
                                 font-weight:700;
                                 letter-spacing:2px;
                                 text-transform:uppercase;
                             ">
                                 Verification Code
                             </p>

                             <div style="
                                 display:inline-block;
                                 background:#ffffff;
                                 padding:16px 36px;
                                 border-radius:16px;
                                 box-shadow:0 6px 20px rgba(155, 93, 229, 0.08);
                                 border: 1px solid #F0E6FD;
                             ">
                                 <span style="
                                     font-size:36px;
                                     font-weight:800;
                                     letter-spacing:10px;
                                     color:#FF9F43;
                                 ">
                                     {verifiedCode}
                                 </span>
                             </div>

                             <p style="
                                 margin:18px 0 0;
                                 color:#A192BD;
                                 font-size:13px;
                             ">
                                 This code is valid for 5 minutes
                             </p>

                         </div>

                         <!-- BUTTON -->
                         <div style="text-align:center; margin:36px 0;">

                             <a href="#"
                                style="
                                     display:inline-block;
                                     background:#9B5DE5;
                                     color:#ffffff;
                                     text-decoration:none;
                                     padding:16px 36px;
                                     border-radius:16px;
                                     font-size:15px;
                                     font-weight:700;
                                     box-shadow:0 8px 20px rgba(155, 93, 229, 0.25);
                                ">
                                 Verify Account
                             </a>

                         </div>

                         <!-- INFO BOX -->
                         <div style="
                             background:#FAF8FD;
                             border:1px solid #EFEAF7;
                             border-left:4px solid #FF9F43;
                             border-radius:14px;
                             padding:16px 20px;
                         ">

                             <p style="
                                 margin:0;
                                 color:#8C7E9F;
                                 font-size:13px;
                                 line-height:1.7;
                             ">
                                 If you did not request this creation, please ignore this email. Your account remains secure.
                             </p>

                         </div>

                     </td>
                 </tr>

                 <!-- FOOTER -->
                 <tr>
                     <td style="
                         background:#FAF8FD;
                         padding:30px 24px;
                         text-align:center;
                         border-top: 1px solid #EFEAF7;
                     ">

                         <h3 style="
                             margin:0 0 8px;
                             color:#2D253B;
                             font-size:18px;
                         ">
                             Mangaka
                         </h3>

                         <p style="
                             margin:0 0 16px;
                             color:#8C7E9F;
                             font-size:13px;
                             line-height:1.6;
                         ">
                             The leading online platform for manga enthusiasts.
                         </p>

                         <p style="
                             margin:0;
                             color:#B2A7C4;
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