using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Cloud.Models.Data;

namespace Cloud.Models {
  public class UserModel : IdentityUser, IBaseEntity {
	[Key]
	public override string Id { get; set; } = Guid.NewGuid().ToString();

	[Required]
	public string FirstName { get; set; } = string.Empty;

	[Required]
	public string LastName { get; set; } = string.Empty;

	public UserRole Role { get; set; }

	public bool IsVerified { get; set; }

	public bool IsBanned { get; set; }

	public string? BanReason { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public string CreatedBy { get; set; } = string.Empty;
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }
	public DateTime? DeletedAt { get; set; }

	public string? ProfilePictureUrl { get; set; } = string.Empty;

	// Navigation properties
	public TenantModel? Tenant { get; set; }
	public OwnerModel? Owner { get; set; }
	public AdminModel? Admin { get; set; }
	public StripeCustomerModel? StripeCustomer { get; set; }

	// Implement IBaseEntity methods
	public void UpdateCreationProperties(DateTime createdAt) {
	  CreatedAt = createdAt;
	}

	public void UpdateModifiedProperties(DateTime? updatedAt) {
	  UpdatedAt = updatedAt;
	}

	public void UpdateIsDeleted(DateTime? deletedAt, bool isDeleted) {
	  IsDeleted = isDeleted;
	  DeletedAt = deletedAt;
	}

	// Implement Guid Id getter for IBaseEntity
	Guid IBaseEntity.Id => Guid.Parse(Id);
  }
}