using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account;

public class LoginModel : PageModel
{
    private readonly InventorySystemDbContext _dbContext;

    public LoginModel(InventorySystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // STEP 1: Validate username and password
        if (string.IsNullOrWhiteSpace(UserName) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        // STEP 2: Retrieve user
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.UserName != null &&
                u.UserName.ToLower() == UserName.ToLower());

        if (user == null)
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        // STEP 3: Check login status
        var loginStatus = await _dbContext.UserLoginInfos
            .Where(x =>
                x.UserId == user.Id &&
                x.Key == "LoginStatus")
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (loginStatus == null)
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        if (loginStatus.Value == "LockedOut")
        {
            ErrorMessage =
                "Your account is LockedOut, have an Admin unlock your account first.";

            return Page();
        }

        // STEP 4: Retrieve stored password
        var storedPassword = await _dbContext.UserLoginInfos
            .Where(x =>
                x.UserId == user.Id &&
                x.Key == "Password")
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (storedPassword == null)
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        // STEP 5: Verify password
        bool passwordMatch = BCrypt.Net.BCrypt.Verify(
            Password,
            storedPassword.Value);

        if (!passwordMatch)
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        // STEP 6: Reset login retries
        var retries = await _dbContext.UserLoginInfos
            .Where(x =>
                x.UserId == user.Id &&
                x.Key == "LoginRetries")
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (retries != null)
        {
            retries.Value = "0";
            await _dbContext.SaveChangesAsync();
        }

        // STEP 7: Retrieve role
        // Your database uses Key = "role"
        var roleInfo = await _dbContext.UserLoginInfos
            .Where(x =>
                x.UserId == user.Id &&
                x.Key.ToLower() == "role")
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        // STEP 8: Normalize role
        string roleValue;

        if (string.Equals(
                roleInfo?.Value?.Trim(),
                "admin",
                StringComparison.OrdinalIgnoreCase))
        {
            roleValue = "Admin";
        }
        else
        {
            roleValue = "User";
        }

        // STEP 9: Create authentication claims
        // Fully qualify Claim to avoid conflict with another Claim class.
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Name,
                user.UserName ?? string.Empty),

            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.GivenName,
                user.FirstName ?? string.Empty),

            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Surname,
                user.LastName ?? string.Empty),

            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Role,
                roleValue)
        };

        // STEP 10: Create identity
        var identity = new System.Security.Claims.ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        // STEP 11: Create principal
        var principal = new System.Security.Claims.ClaimsPrincipal(
            identity);

        // STEP 12: Sign in
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        // STEP 13: Redirect based on role
        if (roleValue == "Admin")
        {
            return RedirectToPage("/Admin/Dashboard");
        }

        return RedirectToPage("/Products/Index");
    }
}