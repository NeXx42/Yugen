using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Yugen.Core.Configs;
using Yugen.Core.Helpers;
using Yugen.Core.Services;
using Yugen.Data;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models;
using Yugen.YugenBackgroundService;
using Yugen.YugenBackgroundService.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "Yugen",

        ValidateAudience = true,
        ValidAudience = "Yugen",

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Encryption:JWTToken"]!)),
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("AuthToken", out string? cookie))
            {
                context.Token = cookie;
            }

            return Task.CompletedTask;
        },

        OnTokenValidated = async context =>
        {
            string? userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return;

            UserService userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
            UserSession user = await userService.GetUser(Guid.Parse(userId));
            context.HttpContext.Items["User"] = user;
        },

        OnAuthenticationFailed = context =>
        {
            Console.WriteLine(context.Exception);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("localhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.Configure<EncryptionConfig>(builder.Configuration.GetSection("Encryption"));

builder.Services.AddDbContext<YugenContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<LibraryService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<HydrationService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddSingleton<CacheService>();
builder.Services.AddSingleton<SettingsCache>();
builder.Services.AddSingleton<EndpointDeduplicator>();

builder.Services.AddHostedService<SettingsInit>();


if (builder.Environment.IsProduction())
{
    builder.Services.AddSingleton<IScheduledJob, LinkDownloadJob>();
    builder.Services.AddSingleton<IScheduledJob, EpisodeNotificationJob>();
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Critical);
}

builder.Services.AddHostedService<YugenBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<YugenContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("localhost");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();