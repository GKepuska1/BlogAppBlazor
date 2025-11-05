namespace BlogApp.Domain.Entities
{
    public class ApplicationUser
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public bool SubscriptionActive { get; set; }
        public DateTime LastPostDate { get; set; }
        public int PostCount { get; set; }
    }
}
