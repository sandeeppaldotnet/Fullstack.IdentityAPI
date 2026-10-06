using Fullstack.IdentityAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fullstack.IdentityAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser constraints
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.FullName).IsRequired();
                b.Property(u => u.AdharNumber).IsRequired();
                b.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // RefreshToken configuration
            builder.Entity<RefreshToken>(b =>
            {
                b.ToTable("RefreshTokens");
                b.HasKey(r => r.Id);
                b.Property(r => r.TokenHash).IsRequired();
                b.Property(r => r.UserId).IsRequired();
                b.Property(r => r.CreatedAtUtc).IsRequired();
                b.Property(r => r.ExpiresAtUtc).IsRequired();
                b.HasIndex(r => r.UserId);
                b.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
