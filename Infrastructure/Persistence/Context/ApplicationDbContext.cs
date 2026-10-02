using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<RefreshToken>()
            //.HasOne(rt => rt.User)
            //.WithMany(u => u.RefreshTokens)
            //.HasForeignKey(rt => rt.UserId);

            //builder.Entity<RefreshToken>()
            //    .HasIndex(rt => rt.Token)
            //    .IsUnique();

            //builder.Entity<Model1>()
            //    .HasMany(m => m.Model2s)
            //    .WithOne()
            //    .HasForeignKey("Model1Id")
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.Entity<Model2>();

        }
    }
}
