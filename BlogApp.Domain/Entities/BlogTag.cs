namespace BlogApp.Domain.Entities
{
    public class BlogTag : BaseEntity
    {
        public int BlogId { get; set; }
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
        public virtual Blog Blog { get; set; }
    }
}
