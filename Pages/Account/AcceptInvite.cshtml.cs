using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;

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
        if (string.IsNullOrWhiteSpace(Token))
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

        // Validate invite token
        var principal = _inviteTokenService.ValidateInviteToken(Token);

        if (principal == null)
        {
            ErrorMessage = "Invalid invite link.";
            return Page();
        }

        // Get UserId from token
        var userIdClaim = principal.FindFirst("UserId");

        if (userIdClaim == null ||
            !int.TryParse(userIdClaim.Value, out int userId))
        {
            ErrorMessage = "Invalid user.";
            return Page();
        }

        // Confirm password
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        // Validate password
        if (!PasswordPolicyHelper.IsValid(Password))
        {
            ErrorMessage =
                "Password must contain uppercase, lowercase, number, " +
                "special character and at least 8 characters.";

            return Page();
        }

        // Hash password
        string hashedPassword =
            BCrypt.Net.BCrypt.HashPassword(Password);

        // Find existing password record
        var passwordRecord = await _dbContext.UserLoginInfos
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Key == "Password");

        if (passwordRecord == null)
        {
            // Create password record
            passwordRecord = new UserLogininfo
            {
                UserId = userId,
                Key = "Password",
                Value = hashedPassword
            };

            _dbContext.UserLoginInfos.Add(passwordRecord);
        }
        else
        {
            // Update existing password
            passwordRecord.Value = hashedPassword;
        }

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Account setup complete. You may now log in.";

        return RedirectToPage("/Account/Login");
    }
}