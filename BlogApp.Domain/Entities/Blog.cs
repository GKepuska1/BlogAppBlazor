namespace BlogApp.Domain.Entities
{
    public class Blog : BaseEntity
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }


        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<BlogTag> BlogTags { get; set; }
    }
}
