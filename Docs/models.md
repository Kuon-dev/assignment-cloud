# Models
This is a list of models provided for a propety listing website. The models are written in C# using Entity Framework Core.

This is Base Entity, almost all of the models inherit this base entity. You are not allowed to directly access the properties
of UpdatedAt and CreatedAt in the base entity, as well as DeletedAt and IsDeleted.
```cs
using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.Data {
  public interface IBaseEntity
  {
	  Guid Id { get; }
	  DateTime CreatedAt { get; }
	  DateTime? UpdatedAt { get; }
	  bool IsDeleted { get; }
	  DateTime? DeletedAt { get; }

	  void UpdateCreationProperties(DateTime createdAt);
	  void UpdateModifiedProperties(DateTime? updatedAt);
	  void UpdateIsDeleted(DateTime? deletedAt, bool isDeleted);
  }

  public abstract class BaseEntity : IBaseEntity
  {
	  [Key]
	  public Guid Id { get; private set; } = Guid.NewGuid();
	  
	  public DateTime CreatedAt { get; private set; }
	  
	  public DateTime? UpdatedAt { get; private set; }

	  public bool IsDeleted { get; private set; }
	  public DateTime? DeletedAt { get; private set; }
	  
	  public void UpdateCreationProperties(DateTime createdAt)
	  {
		  CreatedAt = createdAt;
	  }
	  
	  public void UpdateModifiedProperties(DateTime? updatedAt)
	  {
		  UpdatedAt = updatedAt;
	  }
	  
	  public void UpdateIsDeleted(DateTime? deletedAt, bool isDeleted)
	  {
		  IsDeleted = isDeleted;
	  }
  }
}
```

```cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Cloud.Models.Data;

namespace Cloud.Models
{
    public class UserModel : IdentityUser, IBaseEntity
    {
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
        public void UpdateCreationProperties(DateTime createdAt)
        {
            CreatedAt = createdAt;
        }

        public void UpdateModifiedProperties(DateTime? updatedAt)
        {
            UpdatedAt = updatedAt;
        }

        public void UpdateIsDeleted(DateTime? deletedAt, bool isDeleted)
        {
            IsDeleted = isDeleted;
            DeletedAt = deletedAt;
        }

        // Implement Guid Id getter for IBaseEntity
        Guid IBaseEntity.Id => Guid.Parse(Id);
    }
}
```

```cs
// TenantModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {
  public class TenantModel: BaseEntity {
	[Required]
	public String UserId { get; set; } = null!;

	[ForeignKey("UserId")]
	public UserModel? User { get; set; }

	public Guid? CurrentPropertyId { get; set; }
	[ForeignKey("CurrentPropertyId")]
	public PropertyModel? CurrentProperty { get; set; }

	// Navigation properties
	public ICollection<RentalApplicationModel>? Applications { get; set; }
	public ICollection<RentPaymentModel>? RentPayments { get; set; }
	public ICollection<MaintenanceRequestModel>? MaintenanceRequests { get; set; }
	public ICollection<LeaseModel>? Leases { get; set; }
  }
}
```

```cs
// AdminModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{
    public class AdminModel : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public UserModel? User { get; set; }
    }
}
```

```cs
// OwnerModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {
  public class OwnerModel: BaseEntity {
	[Required]
	public String UserId { get; set; } = null!;

	[ForeignKey("UserId")]
	public UserModel? User { get; set; }

	// Navigation properties
	public ICollection<PropertyModel>? Properties { get; set; }
  }
}
```

```cs
// LeaseModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {

  public class LeaseModel : BaseEntity {

	[Required]
	public Guid TenantId { get; set; }

	[ForeignKey("TenantId")]
	public TenantModel? Tenant { get; set; }

	[Required]
	public Guid PropertyId { get; set; }

	[ForeignKey("PropertyId")]
	public PropertyModel? PropertyModel { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

	[Column(TypeName = "decimal(18,2)")]
	public decimal RentAmount { get; set; }
	[Column(TypeName = "decimal(18,2)")]
	public decimal SecurityDeposit { get; set; }

	public bool IsActive { get; set; }
  }
}
```

```cs
// ListingModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {

  public class ListingModel: BaseEntity {

	[Required]
	public Guid PropertyId { get; set; }

	[ForeignKey("PropertyId")]
	public PropertyModel? Property { get; set; }

	[Required]
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	[Column(TypeName = "decimal(18,2)")]
	public decimal Price { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public bool IsActive { get; set; }
	public int Views { get; set; }
	// Navigation properties
	public ICollection<RentalApplicationModel>? Applications { get; set; }
  }
}
```

```cs
// PropertyModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {

  public class PropertyModel: BaseEntity {
	[Required]
	public Guid OwnerId { get; set; }

	[ForeignKey("OwnerId")]
	public OwnerModel? Owner { get; set; }

	[Required]
	public string Address { get; set; } = string.Empty;

	[Required]
	public string City { get; set; } = string.Empty;

	[Required]
	public string State { get; set; } = string.Empty;

	[Required]
	public string ZipCode { get; set; } = string.Empty;

	public PropertyType PropertyType { get; set; }

	public int Bedrooms { get; set; }

	public int Bathrooms { get; set; }

	/*public float SquareFootage { get; set; }*/

	[Column(TypeName = "decimal(18,2)")]
	public decimal RentAmount { get; set; }

	public string? Description { get; set; }

	public List<string>? Amenities { get; set; }

	public bool IsAvailable { get; set; }

	public RoomType RoomType { get; set; }

	// Navigation properties
	public ICollection<ListingModel>? Listings { get; set; }
	public ICollection<MaintenanceRequestModel>? MaintenanceRequests { get; set; }
  }


  // PropertyTypeModel.cs
  public enum PropertyType {
	Apartment,
	House,
	Condo,
	Townhouse
  }

  public enum RoomType {
	MasterBedroom,
	MiddleBedroom,
	SmallBedroom,
  }
}
```

```cs
// RentalApplicationModel.cs
/*using System;*/
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

/*namespace Cloud.Models*/
namespace Cloud.Models {
  public class RentalApplicationModel: BaseEntity {
	[Required]
	public Guid TenantId { get; set; }

	[ForeignKey("TenantId")]
	public TenantModel? Tenant { get; set; }

	[Required]
	public Guid ListingId { get; set; }

	[ForeignKey("ListingId")]
	public ListingModel? Listing { get; set; }

	public ApplicationStatus Status { get; set; }

	public DateTime ApplicationDate { get; set; }

	public string? EmploymentInfo { get; set; }

	public string? References { get; set; }

	public string? AdditionalNotes { get; set; }
  }

  public enum ApplicationStatus {
	Pending,
	Approved,
	Rejected
  }
}
```

```cs
// RentPaymentModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {

  public class RentPaymentModel: BaseEntity {
	[Required]
	public Guid TenantId { get; set; }

	[ForeignKey("TenantId")]
	public TenantModel? Tenant { get; set; }

	public int Amount { get; set; } // Amount in cents

	[Required]
	public string Currency { get; set; } = "usd";

	[Required]
	public string PaymentIntentId { get; set; } = string.Empty;

	public string? PaymentMethodId { get; set; }

	public PaymentStatus Status { get; set; }
  }

  // PaymentStatusModel.cs
  public enum PaymentStatus {
	RequiresPaymentMethod,
	RequiresConfirmation,
	RequiresAction,
	Processing,
	RequiresCapture,
	Cancelled,
	Succeeded
  }
}
```

```cs
// StripeCustomerModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {

  public class StripeCustomerModel: BaseEntity {
	[Required]
	public String UserId { get; set; } = null!;

	[ForeignKey("UserId")]
	public UserModel? User { get; set; }

	[Required]
	public string StripeCustomerId { get; set; } = string.Empty;
  }
}
```

```cs
using Cloud.Models.Data;

namespace Cloud.Models {
  public class ApplicationDocumentModel: BaseEntity {
	public Guid RentalApplicationId { get; set; }
	public string FileName { get; set; } = null!;
	public string FilePath { get; set; } = null!;
  }

}
```

```cs
// ActivityLogModel.cs
using System.ComponentModel.DataAnnotations;
using Cloud.Models.Data;

namespace Cloud.Models {
  public class ActivityLogModel: BaseEntity {
	[Required]
	public Guid UserId { get; set; }

	[Required]
	public string Action { get; set; } = null!;

	[Required]
	public DateTime Timestamp { get; set; }

	public string Details { get; set; } = string.Empty;
  }
}
```

```cs
// ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Cloud.Models {
  public class ApplicationDbContext : IdentityDbContext<UserModel> {
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options) {
	}

	public DbSet<TenantModel>? Tenants { get; set; }
	public DbSet<OwnerModel>? Owners { get; set; }
	public DbSet<AdminModel>? Admins { get; set; }
	public DbSet<PropertyModel>? Properties { get; set; }
	public DbSet<LeaseModel>? Leases { get; set; }
	public DbSet<RentPaymentModel>? RentPayments { get; set; }
	public DbSet<OwnerPaymentModel> OwnerPayments { get; set; }
	public DbSet<StripeCustomerModel>? StripeCustomers { get; set; }
	public DbSet<MaintenanceRequestModel>? MaintenanceRequests { get; set; }
	public DbSet<MaintenanceTaskModel>? MaintenanceTasks { get; set; }
	public DbSet<ApplicationDocumentModel>? ApplicationDocuments { get; set; }
	public DbSet<ActivityLogModel>? ActivityLogs { get; set; }
	public DbSet<ListingModel> Listings { get; set; }
	public DbSet<RentalApplicationModel> RentalApplications { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
	  base.OnModelCreating(modelBuilder);
	  modelBuilder.Entity<UserModel>()
		  .HasIndex(u => u.Email)
		  .IsUnique();
	  modelBuilder.Entity<StripeCustomerModel>()
		  .HasIndex(sc => sc.StripeCustomerId)
		  .IsUnique();
	  modelBuilder.Entity<UserModel>()
		  .HasOne(u => u.Tenant)
		  .WithOne(t => t!.User)
		  .HasForeignKey<TenantModel>(t => t.UserId);
	  modelBuilder.Entity<UserModel>()
		  .HasOne(u => u.Owner)
		  .WithOne(o => o!.User)
		  .HasForeignKey<OwnerModel>(o => o.UserId);
	  modelBuilder.Entity<UserModel>()
		  .HasOne(u => u.Admin)
		  .WithOne(a => a!.User)
		  .HasForeignKey<AdminModel>(a => a.UserId);
	  modelBuilder.Entity<PropertyModel>()
		  .HasMany(p => p.Listings)
		  .WithOne(l => l!.Property)
		  .OnDelete(DeleteBehavior.Cascade);
	  modelBuilder.Entity<ListingModel>()
		  .HasOne(l => l.Property)
		  .WithMany()
		  .OnDelete(DeleteBehavior.Restrict);
	  /*modelBuilder.Entity<ApplicationDocumentModel>().*/
	}
  }
}
```
