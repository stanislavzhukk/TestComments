using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seeders
{
    public static class UsersSeeder
    {
        public static async Task SeedUsers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            var adminEmail = "admin@example.pl";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Administrator",
                    Surname = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdate = DateTime.UtcNow,
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper(),
                    UserName = "Admin",
                    NormalizedUserName = "ADMIN",
                    EmailConfirmed = true,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            Console.WriteLine($"Admin - {adminUser.Id}");

            var userEmail = "user@example.pl";
            var normalUser = await userManager.FindByEmailAsync(userEmail);
            if (normalUser == null)
            {
                normalUser = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    Surname = "User",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdate = DateTime.UtcNow,
                    Email = userEmail,
                    NormalizedEmail = userEmail.ToUpper(),
                    UserName = "User",
                    NormalizedUserName = "USER",
                    EmailConfirmed = true,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(normalUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
            }
            Console.WriteLine($"Admin - {adminUser.Id}");
            Console.WriteLine($"User - {normalUser.Id}");
        }
    }
}
