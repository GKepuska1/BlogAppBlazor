using BlogApp.Core.Constants;
using BlogApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApp.Core.Services
{
    public interface IJwtService
    {
        public string GenerateToken(ApplicationUser userInfo);
        public List<Claim> ValidateToken(string token);

    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(ApplicationUser userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new Claim(ClaimTypes.Name, userInfo.UserName),
                new Claim(JwtRegisteredClaimNames.GivenName, userInfo.Firstname),
                new Claim(JwtRegisteredClaimNames.FamilyName, userInfo.Lastname),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, userInfo.Id),
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Audience"],
              claims,
              expires: DateTime.UtcNow.AddMinutes(30),
              signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public List<Claim> ValidateToken(string token)
        {
            if (token == null)
                throw new SecurityTokenValidationException("Invalid Token");

            try
            {
                var userClaims = new Dictionary<string, string>();
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

                var validationParameters = TokenValidationConstants.GetValidationParameters(key, _configuration);
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims.ToList();
            }
            catch (Exception ex)
            {
               throw new SecurityTokenValidationException("Invalid Token");
            }
        }
    }
}
