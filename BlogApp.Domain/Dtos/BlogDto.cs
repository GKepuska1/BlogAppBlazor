using System.ComponentModel.DataAnnotations;

namespace BlogApp.Domain.Dtos
{
    public class BlogDto : BlogDtoCreate
    {
        public int Id { get; set; }
        public string User { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CommentDto> Comments { get; set; }
    }

    public class BlogDtoCreate
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Content { get; set; }

        public List<TagDto> Tags { get; set; }
    }
}
