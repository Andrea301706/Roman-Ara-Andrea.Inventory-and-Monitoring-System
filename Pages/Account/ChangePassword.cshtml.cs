using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.DTOs;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using System.Security.Claims;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account;

public class ChangePasswordModel : PageModel
{
    private readonly InventorySystemDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ChangePasswordModel(
        InventorySystemDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [BindProperty]
    public ChangePasswordDto ChangePassword { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // STEP 1: Get UserId from the logged-in user's claims
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // STEP 2: Redirect to Login if UserId is missing
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return RedirectToPage("/Account/Login");
        }

        // Convert UserId to integer
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToPage("/Account/Login");
        }

        // STEP 3: Retrieve User from database
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        // STEP 4: User record not found
        if (user == null)
        {
            return NotFound();
        }

        // STEP 5: Display username
        ChangePassword.Username = user.UserName;

        // STEP 6: Check for profile image
        SetProfileImage(user.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // STEP 1: Get UserId from claims
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // STEP 2: Redirect to Login if UserId is missing
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return RedirectToPage("/Account/Login");
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToPage("/Account/Login");
        }

        // STEP 3: Retrieve User
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        // Display username and profile image again
        ChangePassword.Username = user.UserName;
        SetProfileImage(user.Id);

        // STEP 4: Validate required fields

        if (string.IsNullOrWhiteSpace(
            ChangePassword.CurrentPassword))
        {
            ModelState.AddModelError(
                "ChangePassword.CurrentPassword",
                "Current Password is required.");
        }

        if (string.IsNullOrWhiteSpace(
            ChangePassword.NewPassword))
        {
            ModelState.AddModelError(
                "ChangePassword.NewPassword",
                "New Password is required.");
        }

        if (string.IsNullOrWhiteSpace(
            ChangePassword.ConfirmNewPassword))
        {
            ModelState.AddModelError(
                "ChangePassword.ConfirmNewPassword",
                "Confirm Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // STEP 5: Check password confirmation
        if (ChangePassword.NewPassword !=
            ChangePassword.ConfirmNewPassword)
        {
            ModelState.AddModelError(
                "ChangePassword.ConfirmNewPassword",
                "Passwords do not match.");

            return Page();
        }

        // STEP 6: Retrieve current password
        var passwordRecord =
            await _dbContext.UserLoginInfos
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.Id &&
                    x.Key == "Password");

        if (passwordRecord == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Password record was not found.");

            return Page();
        }

        // STEP 7: Verify current password
        bool currentPasswordCorrect =
            BCrypt.Net.BCrypt.Verify(
                ChangePassword.CurrentPassword,
                passwordRecord.Value);

        if (!currentPasswordCorrect)
        {
            ModelState.AddModelError(
                "ChangePassword.CurrentPassword",
                "Current password is incorrect.");

            return Page();
        }

        // STEP 8: Validate new password policy
        if (!PasswordPolicyHelper.IsValid(
            ChangePassword.NewPassword))
        {
            ModelState.AddModelError(
                "ChangePassword.NewPassword",
                "Password does not meet the required policy. " +
                "Use at least 8 characters with an uppercase letter, " +
                "lowercase letter, number, and special character.");

            return Page();
        }

        // STEP 9: Hash the new password
        var newHashedPassword =
            BCrypt.Net.BCrypt.HashPassword(
                ChangePassword.NewPassword);

        // STEP 10: Update password record
        passwordRecord.Value = newHashedPassword;

        // STEP 11: Save changes
        await _dbContext.SaveChangesAsync();

        // STEP 12: Store success message
        TempData["SuccessMessage"] =
            "Password changed successfully.";

        // STEP 13: Redirect to Profile
        return RedirectToPage("/Account/Profile");
    }

    private void SetProfileImage(int userId)
    {
        var avatarFileName = $"{userId}.png";

        var avatarPath = Path.Combine(
            _environment.WebRootPath,
            "users",
            avatarFileName);

        if (System.IO.File.Exists(avatarPath))
        {
            ChangePassword.ProfileImage =
                $"/users/{avatarFileName}";
        }
        else
        {
            ChangePassword.ProfileImage =
                "/users/default.png";
        }
    }
}