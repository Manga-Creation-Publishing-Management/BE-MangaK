using Manga.Service.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class IdentityController : ControllerBase
{   
    private readonly IService _identityService;
    public IdentityController(IService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
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