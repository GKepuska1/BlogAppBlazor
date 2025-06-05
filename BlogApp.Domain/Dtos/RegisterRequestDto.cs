using System.ComponentModel.DataAnnotations;

namespace BlogApp.Domain.Dtos
{
    public class RegisterRequestDto
    {
        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Lastname { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
