using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;
using System.Security.Claims;

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
            .FirstOrDefault(x =>
                x.UserId == user.Id &&
                x.Key == "LoginStatus");

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
            .FirstOrDefault(x =>
                x.UserId == user.Id &&
                x.Key == "Password");

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
            .FirstOrDefault(x =>
                x.UserId == user.Id &&
                x.Key == "LoginRetries");

        if (retries != null)
        {
            retries.Value = "0";
            _dbContext.SaveChanges();
        }

        // STEP 7: Create Authentication Claims
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.UserName ?? string.Empty),

            new Claim(
                ClaimTypes.GivenName,
                user.FirstName ?? string.Empty),

            new Claim(
                ClaimTypes.Surname,
                user.LastName ?? string.Empty)
        };

        // STEP 8: Create Identity
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        // STEP 9: Create Principal
        var principal = new ClaimsPrincipal(identity);

        // STEP 10: Sign In
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

var authResult = await HttpContext.AuthenticateAsync(
    CookieAuthenticationDefaults.AuthenticationScheme);

Console.WriteLine("===== AUTH TEST AFTER SIGN-IN =====");
Console.WriteLine($"Authenticated: {authResult.Succeeded}");
Console.WriteLine($"User: {authResult.Principal?.Identity?.Name}");
Console.WriteLine(
    $"NameIdentifier: {authResult.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
Console.WriteLine("===================================");

        // STEP 11: Redirect after successful login
        return RedirectToPage("/Products/Index");
    }
}