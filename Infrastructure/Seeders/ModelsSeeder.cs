using Domain.Models;
using Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seeders
{
    public static class ModelsSeeder
    {
        public static async Task SeedModels(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            //if (await context.Model1s.CountAsync() > 0)
            //{
            //    return; // Data already seeded
            //}
            for (int i = 0; i < 3; i++)
            {
                var model = new Model1
                {
                    Id = Guid.NewGuid(),
                    Name = $"Sample Model {i}"
                };
                try
                {
                    await context.AddAsync(model);
                } 
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding Model1: {ex.Message}");
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
