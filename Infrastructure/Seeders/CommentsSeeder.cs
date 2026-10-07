using Domain.Entities;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Seeders
{
    public static class CommentsSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if(context.Comments.Any())
            {
                return;
            }
            for (int i = 1; i <= 50; i++)
            {
                var comment = new Comment
                {
                    UserName = $"User {i}",
                    UserEmail = $"user{i}@example.com",
                    Content = $"This is comment {i}",
                    CreatedAt = DateTime.UtcNow,
                    RootId = null,
                    ParentId = null,
                };
                for(int j = 1; j <= 2; j++)
                {
                    var reply = new Comment
                    {
                        UserName = $"User {j}",
                        UserEmail = $"user{j}@example.com",
                        Content = $"This is a reply to comment {i}",
                        CreatedAt = DateTime.UtcNow,
                        RootId = comment.Id,
                        ParentId = comment.Id,
                    };
                    context.Comments.Add(reply);
                }
                context.Comments.Add(comment);
            }
            await context.SaveChangesAsync();
        }
    }
}
