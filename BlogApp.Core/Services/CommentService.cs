using AutoMapper;
using BlogApp.Core.Data;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;

namespace BlogApp.Core.Services
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetByBlogIdAsync(int blogId);
        Task<CommentDto> CreateAsync(int blogId, CommentDtoCreate comment, string userId);
        Task<CommentDto> UpdateAsync(int id, CommentDtoCreate comment, string userId);
        Task DeleteCommentsByBlogIdAsync(int blogId);
        Task<Comment> GetByIdAsync(int commentId);
        Task DeleteCommentAsync(Comment comment);
    }

    public class CommentService : ICommentService
    {
        private readonly IRedisCommentRepository _commentRepository;
        private readonly IRedisUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CommentService(
            IRedisCommentRepository commentRepository,
            IRedisUserRepository userRepository,
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<CommentDto>> GetByBlogIdAsync(int blogId)
        {
            var comments = await _commentRepository.GetByBlogIdAsync(blogId);

            return comments
                .OrderByDescending(x => x.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Username = c.Username,
                    CreatedAt = c.CreatedAt,
                    IsEdited = c.UpdatedAt > c.CreatedAt
                })
                .ToList();
        }

        public async Task<CommentDto> CreateAsync(int blogId, CommentDtoCreate commentDto, string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException("User not found");

                var comment = new Comment
                {
                    BlogId = blogId,
                    Content = commentDto.Content,
                    UserId = userId,
                    Username = user.UserName
                };

                var created = await _commentRepository.CreateAsync(comment);

                return new CommentDto
                {
                    Id = created.Id,
                    Content = created.Content,
                    Username = created.Username,
                    CreatedAt = created.CreatedAt,
                    IsEdited = false
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating comment", ex);
            }
        }

        public async Task DeleteCommentsByBlogIdAsync(int blogId)
        {
            await _commentRepository.DeleteByBlogIdAsync(blogId);
        }

        public async Task<CommentDto> UpdateAsync(int id, CommentDtoCreate commentDto, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null || comment.UserId != userId)
                throw new KeyNotFoundException("Comment not found");

            comment.Content = commentDto.Content;
            await _commentRepository.UpdateAsync(comment);

            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                Username = comment.Username,
                CreatedAt = comment.CreatedAt,
                IsEdited = comment.UpdatedAt > comment.CreatedAt
            };
        }

        public async Task<Comment> GetByIdAsync(int commentId)
        {
            return await _commentRepository.GetByIdAsync(commentId);
        }

        public async Task DeleteCommentAsync(Comment comment)
        {
            await _commentRepository.DeleteAsync(comment.Id);
        }
    }
}
