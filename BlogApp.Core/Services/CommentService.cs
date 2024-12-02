using AutoMapper;
using BlogApp.Core.Context;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Core.Services
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetByBlogIdAsync(int blogId);
        Task<CommentDto> CreateAsync(int blogId, CommentDtoCreate comment, string userId);

        Task<CommentDto> UpdateAsync(int id, CommentDtoCreate comment, string userId);

        Task DeleteCommentsByBlogIdAsync(int blogId);
    }

    public class CommentService : ICommentService
    {
        private IAppDbContext _dbContext;
        private IMapper _mapper;

        public CommentService(IAppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<CommentDto>> GetByBlogIdAsync(int blogId)
        {
            var comments = await _dbContext.Comments.Where(x => x.BlogId == blogId)
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.User)
                .ToListAsync();

            return _mapper.Map<List<CommentDto>>(comments);
        }

        public async Task<CommentDto> CreateAsync(int blogId, CommentDtoCreate commentDto, string userId)
        {
            try
            {
                var comment = await _dbContext.Comments.AddAsync(new Comment()
                {
                    BlogId = blogId,
                    Content = commentDto.Content,
                    CreatedAt = DateTime.Now,
                    UserId = userId
                });
                await _dbContext.SaveChangesAsync();
                return _mapper.Map<CommentDto>(comment.Entity);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating comment", ex);
            }
        }

        public async Task DeleteCommentsByBlogIdAsync(int blogId)
        {
            var comments = await _dbContext.Comments.Where(x => x.BlogId == blogId).ToListAsync();
            if (comments == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }

            _dbContext.Comments.RemoveRange(comments);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<CommentDto> UpdateAsync(int id, CommentDtoCreate commentDto, string userId)
        {
            var comment = await _dbContext.Comments.FindAsync(id);
            if (comment == null || comment.UserId != userId)
            {
                throw new KeyNotFoundException("Comment not found");
            }

            comment.Content = commentDto.Content;
            comment.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<CommentDto>(comment);
        }

    }
}
