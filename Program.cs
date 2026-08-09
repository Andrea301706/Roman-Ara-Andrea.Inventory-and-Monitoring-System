
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// TEST PASSWORD HASH
// =====================================================
// TEMPORARY ONLY:
// This generates a BCrypt hash for the test password:
// admin123
//
// Run the application once and copy the hash from the
// terminal. Then use that hash in UserLoginInfos.
//
// REMOVE OR COMMENT THIS LINE AFTER COPYING THE HASH.
// =====================================================
Console.WriteLine("TEST PASSWORD HASH:");
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("admin123"));
Console.WriteLine("END TEST PASSWORD HASH");

// =====================================================
// RAZOR PAGES
// =====================================================
builder.Services.AddRazorPages();

// =====================================================
// SESSION
// =====================================================
builder.Services.AddSession();

// =====================================================
// APPLICATION SERVICES
// =====================================================
builder.Services.AddScoped<InviteTokenService>();

builder.Services.AddScoped<
    Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure.EmailService>();

builder.Services.AddScoped<ResetPasswordTokenService>();

// =====================================================
// DATABASE
// =====================================================
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<InventorySystemDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString));
});

// =====================================================
// COOKIE AUTHENTICATION
// =====================================================
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "InventorySystemAuth";

        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;
    });

// =====================================================
// AUTHORIZATION
// =====================================================
builder.Services.AddAuthorization();

var app = builder.Build();

// =====================================================
// ERROR HANDLING
// =====================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// =====================================================
// HTTP PIPELINE
// =====================================================
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// IMPORTANT:
// Authentication MUST come before Authorization.
app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
