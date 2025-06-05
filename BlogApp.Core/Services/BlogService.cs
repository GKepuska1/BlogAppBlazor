using AutoMapper;
using BlogApp.Core.Context;
using BlogApp.Domain.Dtos;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        private IAppDbContext _dbContext;
        private IMapper _mapper;

        public BlogService(IAppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<BlogDto> AddAsync(BlogDtoCreate blogDto, string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

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

            var blog = await _dbContext.Blogs.AddAsync(new Blog()
            var blog = new Blog
            {
                Content = blogDto.Content,
                CreatedAt = DateTime.Now,
                Name = blogDto.Name,
                UserId = userId
            });

            if (!user.SubscriptionActive)
            {
                user.PostCount += 1;
                user.LastPostDate = DateTime.UtcNow.Date;
            }

                UserId = userId,
                BlogTags = new List<BlogTag>()
            };

            var created = await _dbContext.Blogs.AddAsync(blog);

            await _dbContext.SaveChangesAsync();

            if (blogDto.Tags != null && blogDto.Tags.Any())
            {
                foreach (var tagDto in blogDto.Tags)
                {
                    var tagName = tagDto.Name?.Trim().ToLower();
                    if (string.IsNullOrWhiteSpace(tagName))
                    {
                        continue;
                    }

                    var tag = await _dbContext.Tags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName);
                    if (tag == null)
                    {
                        tag = (await _dbContext.Tags.AddAsync(new Tag
                        {
                            Name = tagName,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        })).Entity;
                        await _dbContext.SaveChangesAsync();
                    }

                    blog.BlogTags.Add(new BlogTag
                    {
                        BlogId = blog.Id,
                        TagId = tag.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                await _dbContext.SaveChangesAsync();
            }

            var createdBlogDto = await GetByIdAsync(created.Entity.Id);
            return createdBlogDto;
        }

        public async Task<BlogDto> GetByIdAsync(int blogId)
        {
            var blog = await _dbContext.Blogs
                                       .Include(x => x.User)
                                       .Include(x => x.Comments.OrderByDescending(c => c.CreatedAt))
                                       .Include(x => x.BlogTags)
                                       .ThenInclude(x => x.Tag)
                                       .FirstOrDefaultAsync(x => x.Id == blogId);

            if (blog == null)
            {
                throw new KeyNotFoundException("Blog not found");
            }

            var blogDto = _mapper.Map<BlogDto>(blog);
            return blogDto;
        }

        public async Task<List<BlogDto>> GetPagedResponseAsync(int pageNumber, int pageSize)
        {
            var blogs = await _dbContext.Blogs
                                        .Include(x => x.User)
                                        .Include(x => x.Comments)
                                        .Include(x => x.BlogTags)
                                        .ThenInclude(x => x.Tag)
                                        .OrderByDescending(x => x.CreatedAt)
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            var blogsDto = _mapper.Map<List<BlogDto>>(blogs);
            return blogsDto;
        }

        public async Task<BlogDto> UpdateAsync(int id, BlogDtoCreate blogDto, string userId)
        {
            var blog = await _dbContext.Blogs.FindAsync(id);
            if (blog == null || blog.UserId != userId)
            {
                throw new KeyNotFoundException("Blog not found");
            }

            blog.Name = blogDto.Name;
            blog.Content = blogDto.Content;
            blog.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<BlogDto>(blog);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var blog = await _dbContext.Blogs.FindAsync(id);
            if (blog == null || blog.UserId != userId)
            {
                throw new KeyNotFoundException("Blog not found");
            }
            var blogTags = await _dbContext.BlogTags.Where(bt => bt.BlogId == id).ToListAsync();
            _dbContext.BlogTags.RemoveRange(blogTags);

            _dbContext.Blogs.Remove(blog);
            await _dbContext.SaveChangesAsync();

            foreach (var bt in blogTags)
            {
                var stillUsed = await _dbContext.BlogTags.AnyAsync(x => x.TagId == bt.TagId);
                if (!stillUsed)
                {
                    var tag = await _dbContext.Tags.FindAsync(bt.TagId);
                    if (tag != null)
                    {
                        _dbContext.Tags.Remove(tag);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<BlogDto>> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new ArgumentException("Search term cannot be empty", nameof(term));
            }

            var blogs = await _dbContext.Blogs
                .Where(b => b.Name.Contains(term))
                .Include(x => x.User)
                .Include(x => x.Comments)
                .ToListAsync();

            var blogsDto = _mapper.Map<List<BlogDto>>(blogs);
            return blogsDto;
        }

        public async Task<int> GetTotal()
        {
            return await _dbContext.Blogs.CountAsync();
        }
    }
}
