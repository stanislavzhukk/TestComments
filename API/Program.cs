using API.Extensions;
using Application;
using Infrastructure;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// Add infrastructure services

var uploadsPath = Path.GetFullPath(
    builder.Configuration["FileStorage:UploadsPath"] ?? "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

builder.Services.AddInfrastructure(builder.Configuration);

// Add application services 
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddAuthorization();

// Add API services
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter("captcha", w =>
{
    w.PermitLimit = 20;
    w.Window = TimeSpan.FromMinutes(1);
}));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .GetChildren()
    .Select(section => section.Value)
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Cast<string>()
    .ToArray();

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("AllowedOrigins not configured");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Policy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Comments API", Version = "v1" });
});

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("Policy");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.XContentTypeOptions = "nosniff";

        if (ctx.File.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            ctx.Context.Response.Headers.ContentType = "text/plain; charset=utf-8";
    }
});

app.UseStatusCodePages();

// Apply pending migrations and seed the database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapControllers();

app.Run();
