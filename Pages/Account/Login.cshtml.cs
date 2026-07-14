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
            .FirstOrDefault(u => u.UserName.ToLower() == UserName.ToLower());

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
            ErrorMessage = "Your account is LockedOut, have an Admin unlock your account first";
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

        if (passwordMatch)
        {
            // Reset LoginRetries
            var retries = _dbContext.UserLoginInfos
                .FirstOrDefault(x =>
                    x.UserId == user.Id &&
                    x.Key == "LoginRetries");

            if (retries != null)
            {
                retries.Value = "0";
            }

            _dbContext.SaveChanges();

            // Cookie Authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            // STEP 6: Successful Login
            return RedirectToPage("/Products/Index");
        }

        // STEP 7: Failed Login
        var loginRetries = _dbContext.UserLoginInfos
            .FirstOrDefault(x =>
                x.UserId == user.Id &&
                x.Key == "LoginRetries");

        if (loginRetries == null)
        {
            loginRetries = new UserLoginInfo
            {
                UserId = user.Id,
                Key = "LoginRetries",
                Value = "1"
            };

            _dbContext.UserLoginInfos.Add(loginRetries);
        }
        else
        {
            int retries = int.Parse(loginRetries.Value);
            retries++;
            loginRetries.Value = retries.ToString();
        }

        _dbContext.SaveChanges();

        // STEP 8: Lock Account
        int retryCount = int.Parse(loginRetries.Value);

        if (retryCount > 2)
        {
            loginStatus.Value = "LockedOut";
            _dbContext.SaveChanges();
        }

        ErrorMessage = "Invalid login.";
        return Page();
    }
}