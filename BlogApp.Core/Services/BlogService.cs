using AutoMapper;
using BlogApp.Core.Data;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;

namespace BlogApp.Core.Services
{
    public interface IBlogService
    {
        Task<BlogDto> AddAsync(BlogDtoCreate blogDto, string userId);
        Task<BlogDto> GetByIdAsync(int blogId);
        Task<List<BlogDto>> GetPagedResponseAsync(int pageNumber, int pageSize);
        Task<List<BlogDto>> Search(string term);
        Task<BlogDto> UpdateAsync(int id, BlogDtoCreate blogDto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<int> GetTotal();
    }

    public class BlogService : IBlogService
    {
        private readonly IRedisBlogRepository _blogRepository;
        private readonly IRedisUserRepository _userRepository;
        private readonly IRedisCommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public BlogService(
            IRedisBlogRepository blogRepository,
            IRedisUserRepository userRepository,
            IRedisCommentRepository commentRepository,
            IMapper mapper)
        {
            _blogRepository = blogRepository;
            _userRepository = userRepository;
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        public async Task<BlogDto> AddAsync(BlogDtoCreate blogDto, string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Check daily post limit
            if (!user.SubscriptionActive)
            {
                if (user.LastPostDate.Date == DateTime.UtcNow.Date && user.PostCount >= 1)
                    throw new InvalidOperationException("Daily post limit reached");

                if (user.LastPostDate.Date != DateTime.UtcNow.Date)
                {
                    user.PostCount = 0;
                    user.LastPostDate = DateTime.UtcNow.Date;
                }
            }

            // Create blog
            var blog = new Blog
            {
                Content = blogDto.Content,
                Name = blogDto.Name,
                UserId = userId,
                User = user.UserName
            };

            var created = await _blogRepository.CreateAsync(blog);

            // Update user post count
            if (!user.SubscriptionActive)
            {
                user.PostCount += 1;
                user.LastPostDate = DateTime.UtcNow.Date;
                await _userRepository.UpdateAsync(user);
            }

            // Add tags
            if (blogDto.Tags != null && blogDto.Tags.Any())
            {
                foreach (var tagDto in blogDto.Tags)
                {
                    var tagName = tagDto.Name?.Trim().ToLower();
                    if (string.IsNullOrWhiteSpace(tagName))
                        continue;

                    var tag = new Tag
                    {
                        Name = tagName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _blogRepository.AddTagToBlogAsync(created.Id, tag);
                }
            }

            return await GetByIdAsync(created.Id);
        }

        public async Task<BlogDto> GetByIdAsync(int blogId)
        {
            var blog = await _blogRepository.GetByIdAsync(blogId);
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            // Get tags
            var tags = await _blogRepository.GetTagsForBlogAsync(blogId);

            // Get comments
            var comments = await _commentRepository.GetByBlogIdAsync(blogId);

            var blogDto = new BlogDto
            {
                Id = blog.Id,
                Name = blog.Name,
                Content = blog.Content,
                User = blog.User,
                CreatedAt = blog.CreatedAt,
                Tags = tags.Select(t => new TagDto { Name = t.Name }).ToList(),
                Comments = comments.OrderByDescending(c => c.CreatedAt).Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Username = c.Username,
                    CreatedAt = c.CreatedAt,
                    IsEdited = c.UpdatedAt > c.CreatedAt
                }).ToList()
            };

            return blogDto;
        }

        public async Task<List<BlogDto>> GetPagedResponseAsync(int pageNumber, int pageSize)
        {
            var blogs = await _blogRepository.GetPagedAsync(pageNumber, pageSize);

            var blogDtos = new List<BlogDto>();
            foreach (var blog in blogs)
            {
                var tags = await _blogRepository.GetTagsForBlogAsync(blog.Id);
                var comments = await _commentRepository.GetByBlogIdAsync(blog.Id);

                blogDtos.Add(new BlogDto
                {
                    Id = blog.Id,
                    Name = blog.Name,
                    Content = blog.Content,
                    User = blog.User,
                    CreatedAt = blog.CreatedAt,
                    Tags = tags.Select(t => new TagDto { Name = t.Name }).ToList(),
                    Comments = comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Username = c.Username,
                        CreatedAt = c.CreatedAt,
                        IsEdited = c.UpdatedAt > c.CreatedAt
                    }).ToList()
                });
            }

            return blogDtos;
        }

        public async Task<BlogDto> UpdateAsync(int id, BlogDtoCreate blogDto, string userId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null || blog.UserId != userId)
                throw new KeyNotFoundException("Blog not found");

            blog.Name = blogDto.Name;
            blog.Content = blogDto.Content;

            await _blogRepository.UpdateAsync(blog);

            return await GetByIdAsync(id);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null || blog.UserId != userId)
                throw new KeyNotFoundException("Blog not found");

            // Delete associated comments
            await _commentRepository.DeleteByBlogIdAsync(id);

            // Delete blog
            await _blogRepository.DeleteAsync(id);
        }

        public async Task<List<BlogDto>> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ArgumentException("Search term cannot be empty", nameof(term));

            var blogs = await _blogRepository.SearchByTitleAsync(term);

            var blogDtos = new List<BlogDto>();
            foreach (var blog in blogs)
            {
                var tags = await _blogRepository.GetTagsForBlogAsync(blog.Id);

                blogDtos.Add(new BlogDto
                {
                    Id = blog.Id,
                    Name = blog.Name,
                    Content = blog.Content,
                    User = blog.User,
                    CreatedAt = blog.CreatedAt,
                    Tags = tags.Select(t => new TagDto { Name = t.Name }).ToList()
                });
            }

            return blogDtos;
        }

        public async Task<int> GetTotal()
        {
            return await _blogRepository.GetTotalCountAsync();
        }
    }
}
