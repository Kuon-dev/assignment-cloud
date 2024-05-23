using Microsoft.EntityFrameworkCore;

namespace Cloud.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.EmailVerified).HasDefaultValue(false);
                entity.Property(e => e.BannedUntil).IsRequired(false);

                entity.HasOne(e => e.Profile)
                      .WithOne(p => p.User)
                      .HasForeignKey<Profile>(p => p.UserId)
                      .IsRequired();
            });

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProfileImg).HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(15);

                entity.HasOne(p => p.User)
                      .WithOne(u => u.Profile)
                      .HasForeignKey<Profile>(p => p.UserId)
                      .IsRequired();
            });
        }
    }
}
