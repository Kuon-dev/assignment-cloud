namespace Cloud.Models
{
    public class Profile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ProfileImg { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Foreign key
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        public User User { get; set; } = null!;
    }
}

