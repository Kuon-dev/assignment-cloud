using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Cloud.Models
{
	public class ApplicationDbContext : IdentityDbContext<UserModel>
	{
		public DbSet<TenantModel> Tenants { get; set; } = null!;
		public DbSet<OwnerModel> Owners { get; set; } = null!;
		public DbSet<AdminModel> Admins { get; set; } = null!;
		public DbSet<PropertyModel> Properties { get; set; } = null!;
		public DbSet<LeaseModel> Leases { get; set; } = null!;
		public DbSet<RentPaymentModel> RentPayments { get; set; } = null!;
		public DbSet<OwnerPaymentModel> OwnerPayments { get; set; } = null!;
		public DbSet<StripeCustomerModel> StripeCustomers { get; set; } = null!;
		public DbSet<MaintenanceRequestModel> MaintenanceRequests { get; set; } = null!;
		public DbSet<MaintenanceTaskModel> MaintenanceTasks { get; set; } = null!;
		public DbSet<ApplicationDocumentModel> ApplicationDocuments { get; set; } = null!;
		public DbSet<ActivityLogModel> ActivityLogs { get; set; } = null!;
		public DbSet<ListingModel> Listings { get; set; } = null!;
		public DbSet<RentalApplicationModel> RentalApplications { get; set; } = null!;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
			EnsureDbSetsAreNotNull();
		}

		[MemberNotNull(nameof(Tenants), nameof(Owners), nameof(Admins), nameof(Properties),
					   nameof(Leases), nameof(RentPayments), nameof(OwnerPayments), nameof(StripeCustomers),
					   nameof(MaintenanceRequests), nameof(MaintenanceTasks), nameof(ApplicationDocuments),
					   nameof(ActivityLogs), nameof(Listings), nameof(RentalApplications))]
		private void EnsureDbSetsAreNotNull()
		{
			Tenants = Set<TenantModel>();
			Owners = Set<OwnerModel>();
			Admins = Set<AdminModel>();
			Properties = Set<PropertyModel>();
			Leases = Set<LeaseModel>();
			RentPayments = Set<RentPaymentModel>();
			OwnerPayments = Set<OwnerPaymentModel>();
			StripeCustomers = Set<StripeCustomerModel>();
			MaintenanceRequests = Set<MaintenanceRequestModel>();
			MaintenanceTasks = Set<MaintenanceTaskModel>();
			ApplicationDocuments = Set<ApplicationDocumentModel>();
			ActivityLogs = Set<ActivityLogModel>();
			Listings = Set<ListingModel>();
			RentalApplications = Set<RentalApplicationModel>();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
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
		}
	}
}