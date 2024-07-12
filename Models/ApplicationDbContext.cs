using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
		public DbSet<StripeCustomerModel> StripeCustomers { get; set; } = null!;
		public DbSet<MaintenanceRequestModel> MaintenanceRequests { get; set; } = null!;
		public DbSet<MaintenanceTaskModel> MaintenanceTasks { get; set; } = null!;
		public DbSet<ApplicationDocumentModel> ApplicationDocuments { get; set; } = null!;
		public DbSet<ListingModel> Listings { get; set; } = null!;
		public DbSet<MediaModel> Medias { get; set; } = null!;
		public DbSet<RentalApplicationModel> RentalApplications { get; set; } = null!;
		public DbSet<PayoutPeriod> PayoutPeriods { get; set; } = null!;
		public DbSet<OwnerPayout> OwnerPayouts { get; set; } = null!;
		public DbSet<PayoutSettings> PayoutSettings { get; set; } = null!;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
			EnsureDbSetsAreNotNull();
		}

		[MemberNotNull(nameof(Tenants), nameof(Owners), nameof(Admins), nameof(Properties),
					   nameof(Leases), nameof(RentPayments), nameof(StripeCustomers),
					   nameof(MaintenanceRequests), nameof(MaintenanceTasks), nameof(ApplicationDocuments),
					   nameof(Listings), nameof(RentalApplications), nameof(Medias),
					   nameof(PayoutPeriods), nameof(OwnerPayouts), nameof(PayoutSettings))]
		private void EnsureDbSetsAreNotNull()
		{
			Tenants = Set<TenantModel>();
			Owners = Set<OwnerModel>();
			Admins = Set<AdminModel>();
			Properties = Set<PropertyModel>();
			Leases = Set<LeaseModel>();
			RentPayments = Set<RentPaymentModel>();
			StripeCustomers = Set<StripeCustomerModel>();
			MaintenanceRequests = Set<MaintenanceRequestModel>();
			MaintenanceTasks = Set<MaintenanceTaskModel>();
			ApplicationDocuments = Set<ApplicationDocumentModel>();
			Listings = Set<ListingModel>();
			RentalApplications = Set<RentalApplicationModel>();
			Medias = Set<MediaModel>();
			PayoutPeriods = Set<PayoutPeriod>();
			OwnerPayouts = Set<OwnerPayout>();
			PayoutSettings = Set<PayoutSettings>();
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

			modelBuilder.Entity<OwnerPayout>()
				.HasOne(p => p.Owner)
				.WithMany()
				.HasForeignKey(p => p.OwnerId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<OwnerPayout>()
				.HasOne(p => p.PayoutPeriod)
				.WithMany()
				.HasForeignKey(p => p.PayoutPeriodId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}