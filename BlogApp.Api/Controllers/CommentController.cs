﻿using BlogApp.Core.Services;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class CommentController : ControllerBase
    {
        private IBlogService _blogService;
        private ICommentService _commentService;
        private UserManager<ApplicationUser> _userManager;

        public CommentController(IBlogService blogService,
                                 ICommentService commentService,
                                 UserManager<ApplicationUser> userManager)
        {
            _blogService = blogService;
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpGet("{blogId}")]
        public async Task<IActionResult> Get(int blogId)
        {
            return Ok(await _commentService.GetByBlogIdAsync(blogId));
        }

        [HttpPost("{blogId}")]
        public async Task<IActionResult> Post(int blogId, [FromBody] CommentDtoCreate commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var blog = await _blogService.GetByIdAsync(blogId);
            if (blog == null)
            {
                return NotFound();
            }
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            commentDto = await _commentService.CreateAsync(blogId, commentDto, currentUser.Id);
            return Ok(commentDto);
        }

        [HttpPut("{blogId}/{id}")]
        public async Task<IActionResult> Put(int blogId, int id, [FromBody] CommentDtoCreate commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var blog = await _blogService.GetByIdAsync(blogId);
            if (blog == null)
            {
                return NotFound();
            }
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var comment = await _commentService.UpdateAsync(id, commentDto, currentUser.Id);
            return Ok(comment);
        }


        [HttpDelete("{blogId}/{commentId}")]
        public async Task<IActionResult> Delete(int blogId, int commentId)
        {
            var blog = await _blogService.GetByIdAsync(blogId);
            if (blog == null)
            {
                return NotFound();
            }
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var comment = await _commentService.GetByIdAsync(commentId);
            if (comment == null || comment.BlogId != blog.Id || comment.UserId != currentUser.Id)
            {
                return NotFound();
            }
            await _commentService.DeleteCommentAsync(comment);
            return Ok();
        }
    }
}
