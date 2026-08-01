using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Services;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account;

public class VerifyForgotPasswordModel : PageModel
{
    private readonly InventorySystemDbContext _context;
    private readonly ResetPasswordTokenService _tokenService;

    public VerifyForgotPasswordModel(
        InventorySystemDbContext context,
        ResetPasswordTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [BindProperty]
    public string Token { get; set; } = string.Empty;

    [BindProperty]
    public int UserId { get; set; }

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;


    public IActionResult OnGet(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            ErrorMessage = "Invalid or expired password reset link.";
            return Page();
        }

        var principal = _tokenService.ValidateToken(token);

        if (principal == null)
        {
            ErrorMessage = "Invalid or expired password reset link.";
            return Page();
        }

        var claim = principal.FindFirst("UserId");

        if (claim == null)
        {
            ErrorMessage = "Invalid user.";
            return Page();
        }

        UserId = int.Parse(claim.Value);
        Token = token;

        Console.WriteLine($"TOKEN USER ID: {UserId}");

        return Page();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine("RESET PASSWORD SUBMITTED");
        Console.WriteLine($"UserId: {UserId}");
        Console.WriteLine($"New Password Length: {NewPassword.Length}");


        if (UserId <= 0)
        {
            ErrorMessage = "Invalid user ID.";
            return Page();
        }


        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ModelState.AddModelError("", "Password is required.");
            return Page();
        }


        if (NewPassword != ConfirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return Page();
        }


        if (!PasswordPolicyHelper.IsValid(NewPassword))
        {
            ModelState.AddModelError("", 
                "Password must contain uppercase, lowercase, number, special character and at least 8 characters.");

            return Page();
        }


        var loginInfo = await _context.UserLoginInfos
            .FirstOrDefaultAsync(x =>
                x.UserId == UserId &&
                x.Key == "Password");


        var passwordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);


        if (loginInfo == null)
        {
            Console.WriteLine("Creating new password record.");

            loginInfo = new UserLoginInfo
            {
                UserId = UserId,
                Key = "Password",
                Value = passwordHash
            };

            _context.UserLoginInfos.Add(loginInfo);
        }
        else
        {
            Console.WriteLine("Updating existing password.");

            loginInfo.Value = passwordHash;
        }


        Console.WriteLine("Saving password...");

        await _context.SaveChangesAsync();

        Console.WriteLine("PASSWORD SAVED SUCCESSFULLY");


        TempData["Success"] = "Password changed successfully.";

        return RedirectToPage("/Account/Login");
    }
}



internal class PasswordPolicyHelper
{
    public static bool IsValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (password.Length < 8)
            return false;


        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSpecial = false;


        foreach (char c in password)
        {
            if (char.IsUpper(c))
                hasUpper = true;

            else if (char.IsLower(c))
                hasLower = true;

            else if (char.IsDigit(c))
                hasDigit = true;

            else
                hasSpecial = true;
        }


        return hasUpper &&
               hasLower &&
               hasDigit &&
               hasSpecial;
    }
}