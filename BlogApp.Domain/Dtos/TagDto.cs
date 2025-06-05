using System.ComponentModel.DataAnnotations;

namespace BlogApp.Domain.Dtos
{
    public class TagDto
    {
        [Required]
        public string Name { get; set; }
    }
}
