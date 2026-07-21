using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account;

public class AcceptInviteModel : PageModel
{
    private readonly InventorySystemDbContext _dbContext;
    private readonly InviteTokenService _inviteTokenService;

    public AcceptInviteModel(
        InventorySystemDbContext dbContext,
        InviteTokenService inviteTokenService)
    {
        _dbContext = dbContext;
        _inviteTokenService = inviteTokenService;
    }

    [BindProperty]
    [Required]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(Token))
        {
            ErrorMessage = "Invalid invite link.";
            return Page();
        }

        var principal = _inviteTokenService.ValidateInviteToken(Token);

        if (principal == null)
        {
            ErrorMessage = "Invalid invite link.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // STEP 11 - Validate JWT Token
        var principal = _inviteTokenService.ValidateInviteToken(Token);

        if (principal == null)
        {
            ErrorMessage = "Invalid invite link.";
            return Page();
        }

        // Read UserId from JWT
        var userId = int.Parse(principal.FindFirst("UserId")!.Value);

        // STEP 13 - Confirm Password Validation
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        // STEP 14 - Strong Password Validation
        if (!PasswordPolicyHelper.IsStrongPassword(Password))
        {
            ErrorMessage = "Password does not meet the required policy.";
            return Page();
        }

        // STEP 15 - Hash Password
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

        // STEP 16 - Save Password in UserLoginInfo
        var passwordRecord = await _dbContext.UserLoginInfos
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Key == "Password");

        if (passwordRecord == null)
        {
            passwordRecord = new UserLoginInfo
            {
                UserId = userId,
                Key = "Password",
                Value = hashedPassword
            };

            _dbContext.UserLoginInfos.Add(passwordRecord);
        }
        else
        {
            passwordRecord.Value = hashedPassword;
        }

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Account setup complete. You may now log in.";

        // STEP 17 - Redirect to Login
        return RedirectToPage("/Account/Login");
    }
}