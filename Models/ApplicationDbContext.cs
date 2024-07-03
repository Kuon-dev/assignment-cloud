
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
	/*public DbSet<ListingModel>? Listings { get; set; }*/
	/*public DbSet<RentalApplicationModel>? RentalApplications { get; set; }*/
	public DbSet<LeaseModel>? Leases { get; set; }
	public DbSet<RentPaymentModel>? RentPayments { get; set; }
	public DbSet<StripeCustomerModel>? StripeCustomers { get; set; }
	public DbSet<MaintenanceRequestModel>? MaintenanceRequests { get; set; }
	public DbSet<MaintenanceTaskModel>? MaintenanceTasks { get; set; }
	public DbSet<ApplicationDocumentModel>? ApplicationDocuments { get; set; }

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
	  /*modelBuilder.Entity<ApplicationDocumentModel>().*/
	}
  }
}