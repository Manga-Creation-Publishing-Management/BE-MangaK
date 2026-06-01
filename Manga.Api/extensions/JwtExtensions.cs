using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Manga.Service.JwtService;

namespace Manga.Api.extensions;

public static class JwtExtensions
{
    public const string AdminPolicy = "AdminPolicy";
    public const string MangakaPolicy = "MangakaPolicy";
    public const string AssistantPolicy = "AssistantPolicy";
    public const string TantouEditortPolicy = "TantouEditortPolicy";
    public const string EditorialBoardPolicy = "EditorialBoardPolicy";
    public const string ReaderPolicy = "ReaderPolicy";

    public static void AddJwtServices(this IServiceCollection services, IConfiguration configuration)
    {
        JwtOption jwtOption = new JwtOption();
        configuration.GetSection(nameof(JwtOption)).Bind(jwtOption);
        var key = Encoding.UTF8.GetBytes(jwtOption.SecretKey);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOption.Issuer,
                    ValidAudience = jwtOption.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminPolicy, policy =>
                policy.RequireRole("Admin"));
            // [Authorize(Policy = JwtExtensions.AdminPolicy)]

            options.AddPolicy(MangakaPolicy, policy =>
                policy.RequireRole("Mangaka"));
        
            options.AddPolicy(AssistantPolicy, policy =>
                policy.RequireRole("Assistant"));
            
            options.AddPolicy(TantouEditortPolicy, policy =>
                policy.RequireRole("TantouEditor"));
            
            options.AddPolicy(EditorialBoardPolicy, policy =>
                policy.RequireRole("EditorialBoard"));

            options.AddPolicy(ReaderPolicy, policy =>
                policy.RequireRole("Reader"));
        });
    }
}