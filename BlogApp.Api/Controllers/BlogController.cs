using BlogApp.Core.Services;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class BlogController : ControllerBase
    {
        private IBlogService _blogService;
        private ICommentService _commentService;
        private UserManager<ApplicationUser> _userManager;

        public BlogController(IBlogService blogService,
                              ICommentService commentService,
                              UserManager<ApplicationUser> userManager)
        {
            _blogService = blogService;
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpGet("{page}/{pageSize}")]
        public async Task<IActionResult> Get(int page, int pageSize)
        {
            return Ok(await _blogService.GetPagedResponseAsync(page, pageSize));
        }

        [HttpGet("total")]
        public async Task<IActionResult> Total()
        {
            return Ok(await _blogService.GetTotal());
        }

        [HttpGet("Search/{term}")]
        public async Task<IActionResult> Search(string term)
        {
            return Ok(await _blogService.Search(term));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _blogService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BlogDtoCreate blog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            return Ok(await _blogService.AddAsync(blog, currentUser.Id));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] BlogDtoCreate blog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            return Ok(await _blogService.UpdateAsync(id, blog, currentUser.Id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            await _blogService.DeleteAsync(id, currentUser.Id);
            await _commentService.DeleteCommentsByBlogIdAsync(id);
            return Ok();
        }
    }
}
