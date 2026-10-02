using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.BackgroundServices;
using Infrastructure.Caching;
using Infrastructure.Seeders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Identity;
using Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

//Add DbContext with PostgreSQL provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "HarpagonApp";
});

//Add Identity services
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


//jwt configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:Issuer"];
    options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:Audience"];
    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured"))
    );
});

builder.Services.AddAuthorization();

//Add repositories
builder.Services.AddScoped<IModel1Repository, Model1Repository>();
builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();

//Add services
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IModel1Service, Model1Service>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<ITokenService, JwtService>();

builder.Services.AddHostedService<TokenCleanupService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Input jwt token {token}'"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Apply pending migrations and seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        if(dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying pending migrations...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
        }

        await RolesSeeder.SeedRoles(services);

        if (userManager.Users.Count() < 2)
        {
            await UsersSeeder.SeedUsers(services);
        }
        if (!dbContext.Model1s.Any())
        {
            await ModelsSeeder.SeedModels(services);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"error during migration/seeds: {ex.Message} | {ex.InnerException}");
    }
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
