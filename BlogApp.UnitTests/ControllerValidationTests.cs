using BlogApp.Api.Controllers;
using BlogApp.Core.Services;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BlogApp.UnitTests
{
    public class ControllerValidationTests
    {
        private Mock<UserManager<ApplicationUser>> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequest()
        {
            var userManager = GetUserManagerMock();
            var jwtService = new Mock<IJwtService>();
            var controller = new AuthController(userManager.Object, jwtService.Object);
            controller.ModelState.AddModelError("Username", "Required");

            var result = await controller.Login(new LoginRequestDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequest()
        {
            var userManager = GetUserManagerMock();
            var jwtService = new Mock<IJwtService>();
            var controller = new AuthController(userManager.Object, jwtService.Object);
            controller.ModelState.AddModelError("Username", "Required");

            var result = await controller.Register(new RegisterRequestDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task BlogPost_InvalidModel_ReturnsBadRequest()
        {
            var blogService = new Mock<IBlogService>();
            var commentService = new Mock<ICommentService>();
            var userManager = GetUserManagerMock();
            var controller = new BlogController(blogService.Object, commentService.Object, userManager.Object);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Post(new BlogDtoCreate());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CommentPost_InvalidModel_ReturnsBadRequest()
        {
            var blogService = new Mock<IBlogService>();
            var commentService = new Mock<ICommentService>();
            var userManager = GetUserManagerMock();
            var controller = new CommentController(blogService.Object, commentService.Object, userManager.Object);
            controller.ModelState.AddModelError("Content", "Required");

            var result = await controller.Post(1, new CommentDtoCreate());

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
