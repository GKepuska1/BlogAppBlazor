using BlogApp.Core.Services;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private IJwtService _jwtService;

        public AuthController(UserManager<ApplicationUser> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            var loginResult = await _userManager.CheckPasswordAsync(user, request.Password);
            if (loginResult)
            {
                var token = _jwtService.GenerateToken(user);
                return Ok(token);
            }
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = request.Username,
                    Firstname = request.Firstname,
                    Lastname = request.Lastname
                }, request.Password);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(request.Username);
                    var token = _jwtService.GenerateToken(user);
                    return Ok(token);
                }
                return Unauthorized(result.Errors.Select(x => x.Description));
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }
    }
}
