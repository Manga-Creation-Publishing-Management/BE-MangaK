using System.ComponentModel.DataAnnotations;

namespace Manga.Service.JwtService;

public class JwtOptions
{
    [Required]  public string Issuer { get; set; } = string.Empty;
    [Required]  public string Audience { get; set; } =  string.Empty;
    [Required] public string SecretKey { get; set; } = string.Empty;
    [Required] public int Expiration { get; set; } = 0;
}