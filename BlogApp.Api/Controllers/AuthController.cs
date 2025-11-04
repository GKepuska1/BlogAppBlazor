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
        private IGuestNameGenerator _guestNameGenerator;

        public AuthController(UserManager<ApplicationUser> userManager, IJwtService jwtService, IGuestNameGenerator guestNameGenerator)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _guestNameGenerator = guestNameGenerator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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

        [HttpPost("guest")]
        public async Task<IActionResult> GuestLogin()
        {
            var guestName = _guestNameGenerator.GenerateGuestName();
            var defaultPassword = "Guest123!"; // Simple password for guest users

            // Create guest user
            var guestUser = new ApplicationUser
            {
                UserName = guestName,
                Firstname = "Guest",
                Lastname = guestName
            };

            var result = await _userManager.CreateAsync(guestUser, defaultPassword);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(guestName);
                var token = _jwtService.GenerateToken(user);
                return Ok(new { token, username = guestName });
            }

            // If guest name already exists (very unlikely), try again
            return await GuestLogin();
        }
    }
}
