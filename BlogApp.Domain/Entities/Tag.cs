namespace BlogApp.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }
        public virtual ICollection<BlogTag> BlogTags { get; set; }
    }
}
