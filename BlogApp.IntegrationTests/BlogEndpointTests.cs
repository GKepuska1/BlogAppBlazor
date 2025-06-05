using BlogApp.Api.Controllers;
using BlogApp.Core.Services;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

namespace BlogApp.IntegrationTests
{
    public class BlogEndpointTests
    {
        private readonly TestSettings _settings;

        public BlogEndpointTests()
        {
            _settings = TestServicesConfiguration.ConfigureInMemoryDatabaseAndServices();
        }

        [Fact]
        public async Task GetTotal_ReturnsOk()
        {
            // Arrange
            var blogService = _settings.ServiceProvider.GetRequiredService<IBlogService>();
            var commentService = _settings.ServiceProvider.GetRequiredService<ICommentService>();
            var userManager = _settings.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var controller = new BlogController(blogService, commentService, userManager);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "test") }, "Test"))
                }
            };

            // Act
            var result = await controller.Total();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
    }
}
