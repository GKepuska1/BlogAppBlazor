using BlogApp.Core.Data;
using BlogApp.Core.Services;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRedisUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IGuestNameGenerator _guestNameGenerator;

        public AuthController(
            IRedisUserRepository userRepository,
            IJwtService jwtService,
            IGuestNameGenerator guestNameGenerator)
        {
            _userRepository = userRepository;
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

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized();
            }

            var loginResult = await _userRepository.CheckPasswordAsync(user, request.Password);
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
                // Check if username already exists
                var existing = await _userRepository.GetByUsernameAsync(request.Username);
                if (existing != null)
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                var user = await _userRepository.CreateAsync(new ApplicationUser
                {
                    UserName = request.Username,
                    Firstname = request.Firstname,
                    Lastname = request.Lastname
                }, request.Password);

                var token = _jwtService.GenerateToken(user);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create user" });
            }
        }

        [HttpPost("guest")]
        public async Task<IActionResult> GuestLogin()
        {
            var guestName = _guestNameGenerator.GenerateGuestName();
            var defaultPassword = "Guest123!";

            // Create guest user
            var guestUser = new ApplicationUser
            {
                UserName = guestName,
                Firstname = "Guest",
                Lastname = guestName
            };

            try
            {
                var user = await _userRepository.CreateAsync(guestUser, defaultPassword);
                var token = _jwtService.GenerateToken(user);
                return Ok(new { token, username = guestName });
            }
            catch
            {
                // If guest name already exists (very unlikely), try again
                return await GuestLogin();
            }
        }
    }
}
