using BlogApp.Core.Services;
using BlogApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Xunit;

namespace BlogApp.UnitTests
{
    public class JwtServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                    {"Jwt:Key",       "YourInMemoryTestKeyForJwtTest123"},
                    {"Jwt:Issuer",    "TestIssuer"},
                    {"Jwt:Audience",  "TestAudience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _jwtService = new JwtService(_configuration);
        }

        [Fact]
        public void GenerateToken_ShouldReturnNonEmptyString()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testUser",
                Firstname = "Test",
                Lastname = "User",
                Id = "123"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void ValidateToken_WithValidToken_ShouldReturnClaims()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testUser",
                Firstname = "Test",
                Lastname = "User",
                Id = "123"
            };
            var token = _jwtService.GenerateToken(user);

            // Act
            var claims = _jwtService.ValidateToken(token);

            // Assert
            Assert.NotNull(claims);
            Assert.Contains(claims, c => c.Type == ClaimTypes.Name && c.Value == "testUser");
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ShouldThrow()
        {
            // Arrange
            var invalidToken = "invalidTokenBuddy";

            // Act & Assert
            Assert.ThrowsAny<SecurityTokenValidationException>(() => _jwtService.ValidateToken(invalidToken));
        }

        [Fact]
        public void ValidateToken_WithNullToken_ShouldThrow()
        {
            // Act & Assert
            Assert.ThrowsAny<SecurityTokenValidationException>(() => _jwtService.ValidateToken(null));
        }
    }
}
