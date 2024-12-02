namespace BlogApp.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; }
        public string UserId { get; set; }
        public int BlogId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Blog Blog { get; set; }
    }
}
