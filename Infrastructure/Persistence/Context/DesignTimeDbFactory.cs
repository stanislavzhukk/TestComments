using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence.Context
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "database";
            var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "user";
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "123456";

            optionsBuilder.UseNpgsql($"Host={host};Port={port};Database={database};Username={user};Password={password}");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}