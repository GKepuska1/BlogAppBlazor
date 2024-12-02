using Microsoft.AspNetCore.Identity;

namespace BlogApp.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
