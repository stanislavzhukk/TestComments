using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Model2> Model2s { get; set; }
        public DbSet<Model1> Model1s { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            builder.Entity<Model1>()
                .HasMany(m => m.Model2s)
                .WithOne()
                .HasForeignKey("Model1Id")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Model2>();

        }
    }
}
