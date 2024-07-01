// UserModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Cloud.Models {
  public class UserModel
  {
      [Key]
      public Guid Id { get; set; }

      [Required]
      [EmailAddress]
      public string Email { get; set; } = string.Empty;

      [Required]
      public string Password { get; set; } = string.Empty;

      [Required]
      public string FirstName { get; set; } = string.Empty;

      [Required]
      public string LastName { get; set; } = string.Empty;

      public string? PhoneNumber { get; set; }

      public UserRole Role { get; set; }

      public bool IsVerified { get; set; }

      public bool IsBanned { get; set; }

      public string? BanReason { get; set; }

      public DateTime CreatedAt { get; set; }

      public DateTime UpdatedAt { get; set; }

      public DateTime? DeletedAt { get; set; }

      // Navigation properties
      public TenantModel? Tenant { get; set; }
      public OwnerModel? Owner { get; set; }
      public AdminModel? Admin { get; set; }
      /*public MaintenanceStaffModel? MaintenanceStaff { get; set; }*/
      public StripeCustomerModel? StripeCustomer { get; set; }
  }
}


