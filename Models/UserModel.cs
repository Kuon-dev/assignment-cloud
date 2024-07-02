// UserModel.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Cloud.Models
{
    public class UserModel : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsVerified { get; set; }

        public bool IsBanned { get; set; }

        public string? BanReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? ProfilePictureUrl { get; set; } = string.Empty;

        // Navigation properties
        public TenantModel? Tenant { get; set; }
        public OwnerModel? Owner { get; set; }
        public AdminModel? Admin { get; set; }
        public StripeCustomerModel? StripeCustomer { get; set; }
    }
}

