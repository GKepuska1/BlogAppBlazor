namespace BlogApp.Domain.Entities
{
    public class Blog : BaseEntity
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public string User { get; set; } // Username for display
    }
}
