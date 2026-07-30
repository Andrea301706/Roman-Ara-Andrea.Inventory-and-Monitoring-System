using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Services;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("admin123"));

// Add Razor Pages
builder.Services.AddRazorPages();

// Add Session
builder.Services.AddSession();

// Register InviteTokenService
builder.Services.AddScoped<InviteTokenService>();

// ✅ Register EmailService
builder.Services.AddScoped<Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure.EmailService>();

builder.Services.AddScoped<ResetPasswordTokenService>();

// Database
builder.Services.AddDbContext<InventorySystemDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    ));

// Cookie Authentication
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();