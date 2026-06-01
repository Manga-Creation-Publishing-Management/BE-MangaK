using Manga.Service.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{   
    private readonly IService _identityService;
    public AuthController(IService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Request request)
    {
        try
        {
            var result = await _identityService.Login(request);
            return Ok(result);

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}