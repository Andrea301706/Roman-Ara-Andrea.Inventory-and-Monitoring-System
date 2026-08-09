using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account;

public class InviteModel : PageModel
{
    private readonly InventorySystemDbContext _dbContext;
    private readonly InviteTokenService _inviteTokenService;
    private readonly EmailService _emailService;

    public InviteModel(
        InventorySystemDbContext dbContext,
        InviteTokenService inviteTokenService,
        EmailService emailService)
    {
        _dbContext = dbContext;
        _inviteTokenService = inviteTokenService;
        _emailService = emailService;
    }

    [BindProperty]
    public UserInviteDto UserInviteDto { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Check existing username
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.UserName!.ToLower() == UserInviteDto.Username!.ToLower());

        if (existingUser != null)
        {
            ModelState.AddModelError(
                "UserInviteDto.Username",
                "Username already exists.");

            return Page();
        }


        // Age validation
        DateTime birthDate = UserInviteDto.DateOfBirth!.Value;

        int age = DateTime.Today.Year - birthDate.Year;

        if (birthDate.Date > DateTime.Today.AddYears(-age))
        {
            age--;
        }

        if (age <= 17)
        {
            ModelState.AddModelError(
                "UserInviteDto.DateOfBirth",
                "User must be more than 17 years old.");

            return Page();
        }


        // Create User
        var newUser = new User
        {
            UserName = UserInviteDto.Username,
            FirstName = UserInviteDto.FirstName,
            LastName = UserInviteDto.LastName,
            DateOfBirth = UserInviteDto.DateOfBirth.Value
        };


        _dbContext.Users.Add(newUser);

        await _dbContext.SaveChangesAsync();


        // Create login information
        _dbContext.UserLoginInfos.Add(new UserLoginInfo
        {
            UserId = newUser.Id,
            Key = "LoginStatus",
            Value = "Pending"
        });


        await _dbContext.SaveChangesAsync();


        // Create invite token
        var token = _inviteTokenService.CreateInviteToken(newUser.Id);


        // Create invite URL
        var inviteUrl =
            $"https://localhost:7180/Account/AcceptInvite?token={token}";


        // Send email
        await _emailService.SendInviteEmailAsync(
            UserInviteDto.Email!,
            inviteUrl);


        TempData["SuccessMessage"] =
            "Invitation email sent successfully!";

        TempData["InviteUrl"] = inviteUrl;


        return Page();
    }
}


// DTO used by the invite form
public class UserInviteDto
{
    [Required(ErrorMessage = "Username is required.")]
    public string? Username { get; set; }


    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }


    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }


    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }


    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateTime? DateOfBirth { get; set; }
}