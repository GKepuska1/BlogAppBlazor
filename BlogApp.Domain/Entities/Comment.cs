namespace BlogApp.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; } // Username for display
        public int BlogId { get; set; }
    }
}
