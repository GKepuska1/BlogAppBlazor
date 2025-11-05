using BlogApp.Domain.Entities;

namespace BlogApp.Core.Data
{
    public interface IRedisBlogRepository
    {
        Task<Blog> GetByIdAsync(int id);
        Task<Blog> CreateAsync(Blog blog);
        Task<bool> UpdateAsync(Blog blog);
        Task<bool> DeleteAsync(int id);
        Task<List<Blog>> GetPagedAsync(int page, int pageSize);
        Task<List<Blog>> SearchByTitleAsync(string term);
        Task<int> GetTotalCountAsync();
        Task<List<Tag>> GetTagsForBlogAsync(int blogId);
        Task AddTagToBlogAsync(int blogId, Tag tag);
    }

    public class RedisBlogRepository : IRedisBlogRepository
    {
        private readonly IRedisService _redis;
        private const string BLOG_PREFIX = "blog:";
        private const string BLOG_LIST = "blogs:list";
        private const string BLOG_COUNTER = "blogs:counter";
        private const string TAG_PREFIX = "tag:";
        private const string BLOG_TAGS_PREFIX = "blog:tags:";

        public RedisBlogRepository(IRedisService redis)
        {
            _redis = redis;
        }

        public async Task<Blog> GetByIdAsync(int id)
        {
            var blog = await _redis.GetAsync<Blog>($"{BLOG_PREFIX}{id}");
            return blog;
        }

        public async Task<Blog> CreateAsync(Blog blog)
        {
            // Generate ID
            var id = (int)await _redis.IncrementAsync(BLOG_COUNTER);
            blog.Id = id;
            blog.CreatedAt = DateTime.UtcNow;
            blog.UpdatedAt = DateTime.UtcNow;

            // Save blog
            await _redis.SetAsync($"{BLOG_PREFIX}{id}", blog);

            // Add to list (for pagination)
            var db = _redis.GetDatabase();
            await db.ListLeftPushAsync(BLOG_LIST, id);

            return blog;
        }

        public async Task<bool> UpdateAsync(Blog blog)
        {
            var existing = await GetByIdAsync(blog.Id);
            if (existing == null)
                return false;

            blog.UpdatedAt = DateTime.UtcNow;
            await _redis.SetAsync($"{BLOG_PREFIX}{blog.Id}", blog);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = _redis.GetDatabase();

            // Remove from list
            await db.ListRemoveAsync(BLOG_LIST, id);

            // Delete blog
            var deleted = await _redis.DeleteAsync($"{BLOG_PREFIX}{id}");

            // Delete tags association
            await _redis.DeleteAsync($"{BLOG_TAGS_PREFIX}{id}");

            return deleted;
        }

        public async Task<List<Blog>> GetPagedAsync(int page, int pageSize)
        {
            var db = _redis.GetDatabase();
            var start = (page - 1) * pageSize;
            var stop = start + pageSize - 1;

            var blogIds = await db.ListRangeAsync(BLOG_LIST, start, stop);

            var blogs = new List<Blog>();
            foreach (var id in blogIds)
            {
                var blog = await GetByIdAsync((int)id);
                if (blog != null)
                    blogs.Add(blog);
            }

            return blogs;
        }

        public async Task<List<Blog>> SearchByTitleAsync(string term)
        {
            var allKeys = await _redis.GetKeysByPatternAsync($"{BLOG_PREFIX}*");
            var blogs = new List<Blog>();

            foreach (var key in allKeys)
            {
                var blog = await _redis.GetAsync<Blog>(key);
                if (blog != null && blog.Name != null &&
                    blog.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    blogs.Add(blog);
                }
            }

            return blogs.OrderByDescending(b => b.CreatedAt).ToList();
        }

        public async Task<int> GetTotalCountAsync()
        {
            var db = _redis.GetDatabase();
            var count = await db.ListLengthAsync(BLOG_LIST);
            return (int)count;
        }

        public async Task<List<Tag>> GetTagsForBlogAsync(int blogId)
        {
            return await _redis.GetAsync<List<Tag>>($"{BLOG_TAGS_PREFIX}{blogId}") ?? new List<Tag>();
        }

        public async Task AddTagToBlogAsync(int blogId, Tag tag)
        {
            var tags = await GetTagsForBlogAsync(blogId);

            // Check if tag already exists
            if (!tags.Any(t => t.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
            {
                tag.Id = tags.Count + 1;
                tags.Add(tag);
                await _redis.SetAsync($"{BLOG_TAGS_PREFIX}{blogId}", tags);
            }
        }
    }
}
