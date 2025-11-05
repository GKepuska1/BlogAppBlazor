using BlogApp.Domain.Entities;

namespace BlogApp.Core.Data
{
    public interface IRedisCommentRepository
    {
        Task<Comment> GetByIdAsync(int id);
        Task<Comment> CreateAsync(Comment comment);
        Task<bool> UpdateAsync(Comment comment);
        Task<bool> DeleteAsync(int id);
        Task<List<Comment>> GetByBlogIdAsync(int blogId);
        Task DeleteByBlogIdAsync(int blogId);
    }

    public class RedisCommentRepository : IRedisCommentRepository
    {
        private readonly IRedisService _redis;
        private const string COMMENT_PREFIX = "comment:";
        private const string COMMENT_COUNTER = "comments:counter";
        private const string BLOG_COMMENTS_PREFIX = "blog:comments:";

        public RedisCommentRepository(IRedisService redis)
        {
            _redis = redis;
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
            return await _redis.GetAsync<Comment>($"{COMMENT_PREFIX}{id}");
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            // Generate ID
            var id = (int)await _redis.IncrementAsync(COMMENT_COUNTER);
            comment.Id = id;
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            // Save comment
            await _redis.SetAsync($"{COMMENT_PREFIX}{id}", comment);

            // Add to blog's comments list
            var comments = await GetByBlogIdAsync(comment.BlogId);
            comments.Add(comment);
            await _redis.SetAsync($"{BLOG_COMMENTS_PREFIX}{comment.BlogId}", comments);

            return comment;
        }

        public async Task<bool> UpdateAsync(Comment comment)
        {
            var existing = await GetByIdAsync(comment.Id);
            if (existing == null)
                return false;

            comment.UpdatedAt = DateTime.UtcNow;
            await _redis.SetAsync($"{COMMENT_PREFIX}{comment.Id}", comment);

            // Update in blog's comments list
            var comments = await GetByBlogIdAsync(comment.BlogId);
            var index = comments.FindIndex(c => c.Id == comment.Id);
            if (index >= 0)
            {
                comments[index] = comment;
                await _redis.SetAsync($"{BLOG_COMMENTS_PREFIX}{comment.BlogId}", comments);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var comment = await GetByIdAsync(id);
            if (comment == null)
                return false;

            // Remove from blog's comments list
            var comments = await GetByBlogIdAsync(comment.BlogId);
            comments.RemoveAll(c => c.Id == id);
            await _redis.SetAsync($"{BLOG_COMMENTS_PREFIX}{comment.BlogId}", comments);

            // Delete comment
            return await _redis.DeleteAsync($"{COMMENT_PREFIX}{id}");
        }

        public async Task<List<Comment>> GetByBlogIdAsync(int blogId)
        {
            var comments = await _redis.GetAsync<List<Comment>>($"{BLOG_COMMENTS_PREFIX}{blogId}");
            return comments ?? new List<Comment>();
        }

        public async Task DeleteByBlogIdAsync(int blogId)
        {
            var comments = await GetByBlogIdAsync(blogId);

            foreach (var comment in comments)
            {
                await _redis.DeleteAsync($"{COMMENT_PREFIX}{comment.Id}");
            }

            await _redis.DeleteAsync($"{BLOG_COMMENTS_PREFIX}{blogId}");
        }
    }
}
