using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using System.Security.Claims;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;

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
        // STEP 1: Validate Username and Password
        if (string.IsNullOrWhiteSpace(UserName) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        // STEP 2: Retrieve User
        var user = _dbContext.Users
            .FirstOrDefault(u =>
                u.UserName.ToLower() == UserName.ToLower());

        if (user == null)
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        // STEP 3: Check Login Status
        var loginStatus = _dbContext.UserLoginInfos
            .Where(x =>
                x.UserId == user.Id &&
                x.Key == "LoginStatus")
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

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

        // STEP 4: Retrieve Stored Password
        var storedPassword = _dbContext.UserLoginInfos
            .Where(x =>
                x.UserId == user.Id &&
                x.Key == "Password")
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (storedPassword == null)
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        // STEP 5: Verify Password
        bool passwordMatch = BCrypt.Net.BCrypt.Verify(
            Password,
            storedPassword.Value);

        if (!passwordMatch)
        {
            ErrorMessage = "Invalid login.";
            return Page();
        }

        // STEP 6: Reset Login Retries
        var retries = _dbContext.UserLoginInfos
            .Where(x =>
                x.UserId == user.Id &&
                x.Key == "LoginRetries")
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (retries != null)
        {
            retries.Value = "0";
            await _dbContext.SaveChangesAsync();
        }

        // STEP 7: Retrieve User Role
        var roleInfo = _dbContext.UserLoginInfos
            .Where(x =>
                x.UserId == user.Id &&
                x.Key.ToLower() == "role")
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        // STEP 8: Normalize Role
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

        // STEP 9: Create Authentication Claims
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.UserName),

            new Claim(
                ClaimTypes.GivenName,
                user.FirstName),

            new Claim(
                ClaimTypes.Surname,
                user.LastName),

            new Claim(
                ClaimTypes.Role,
                roleValue)
        };

        // STEP 10: Create Identity
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        // STEP 11: Create Principal
        var principal = new ClaimsPrincipal(identity);

        // STEP 12: Sign In
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        // STEP 13: Redirect based on user role
        if (roleValue == "Admin")
        {
            return RedirectToPage("/Admin/Dashboard");
        }

        return RedirectToPage("/Products/Index");
    }
}

