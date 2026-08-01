using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Services;
using EmailService = Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure.EmailService;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly InventorySystemDbContext _context;
    private readonly EmailService _emailService;
    private readonly ResetPasswordTokenService _tokenService;

    public ForgotPasswordModel(
        InventorySystemDbContext context,
        EmailService emailService,
        ResetPasswordTokenService tokenService)
    {
        _context = context;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    [BindProperty]
    public string EmailAddress { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(EmailAddress))
        {
            ModelState.AddModelError("", "Email address is required.");
            return Page();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == EmailAddress);

        if (user != null)
        {
            try
            {
                var token = _tokenService.GenerateResetToken(user.Id);

                var link = $"{Request.Scheme}://{Request.Host}/account/verify-forgot-password?token={token}";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Reset Password",
                    $@"
Hello {user.FirstName},<br><br>c

You requested to reset your password.<br><br>

Click the link below to reset your password:<br><br>

<a href='{link}'>{link}</a><br><br>

This link will expire in <b>5 minutes</b>.<br><br>

If you did not request this password reset, you may ignore this email.");
            }
            catch
            {
                // Optional: log the exception here.
                // Do not expose email sending errors to the user.
            }
        }

        // Always return the same message for security
        Message = "We have sent you an email with link to reset your password.";

        return Page();
    }
}