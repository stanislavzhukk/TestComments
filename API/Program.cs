using API.Extensions;
//using Application.Services;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

//Add DbContext with PostgreSQL provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddAuthorization();

//Add services
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Comments API", Version = "v1" });

    //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    //c.CustomSchemaIds(t => t.FullName);
});

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Apply pending migrations and seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying pending migrations...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
        }
    }
    catch(Exception ex)
    {
        
        throw new Exception($"Error during migration: {ex.Message} | {ex.InnerException}");
    }

        //    await RolesSeeder.SeedRoles(services);

        //    if (userManager.Users.Count() < 2)
        //    {
        //        await UsersSeeder.SeedUsers(services);
        //    }
        //    if (!dbContext.Model1s.Any())
        //    {
        //        await ModelsSeeder.SeedModels(services);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"error during migration/seeds: {ex.Message} | {ex.InnerException}");
        //}
    }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
