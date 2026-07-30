using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;
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
    public string Token { get; set; } = "";

    [BindProperty]
    public int UserId { get; set; }

    [BindProperty]
    public string NewPassword { get; set; } = "";

    [BindProperty]
    public string ConfirmPassword { get; set; } = "";

    public string ErrorMessage { get; set; } = "";

    public IActionResult OnGet(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            ErrorMessage = "Invalid or expired password reset link.";
            return Page();
        }

        var principal = GetPrincipalFromResetPasswordToken(token);

        if (principal == null)
        {
            ErrorMessage = "Invalid or expired password reset link.";
            return Page();
        }

        var claim = principal.FindFirst("UserId");

        if (claim == null)
        {
            ErrorMessage = "Invalid or expired password reset link.";
            return Page();
        }

        UserId = int.Parse(claim.Value);
        Token = token;

        return Page();
    }

    private System.Security.Claims.ClaimsPrincipal? GetPrincipalFromResetPasswordToken(string token)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return Page();
        }

        if (!PasswordPolicyHelper.IsValid(NewPassword))
        {
            ModelState.AddModelError("", "Password does not meet the required policy.");
            return Page();
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);

        var loginInfo = await _context.UserLoginInfos
            .FirstOrDefaultAsync(x => x.UserId == UserId && x.Key == "Password");

        if (loginInfo == null)
        {
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
            loginInfo.Value = passwordHash;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Your password has been reset successfully.";

        return Redirect("/account/login");
    }
}

internal class PasswordPolicyHelper
{
    public static bool IsValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (password.Length < 8)
        {
            return false;
        }

        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSpecial = false;

        foreach (var c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSpecial = true;
        }

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}