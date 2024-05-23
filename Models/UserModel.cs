// using System;
// using System.Collections.Generic;

namespace Cloud.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool EmailVerified { get; set; } = false;
        public DateTime? BannedUntil { get; set; }

        // Navigation property
        public Profile? Profile { get; set; }
    }
}

