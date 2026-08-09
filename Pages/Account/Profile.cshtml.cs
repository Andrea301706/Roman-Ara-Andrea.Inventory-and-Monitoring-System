using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly InventorySystemDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ProfileModel(
        InventorySystemDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public UserDto UserDto { get; set; } = new UserDto();

    public IActionResult OnGet()
    {
        // Get UserId from the logged-in user's claims
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Content("Unable to resolve user profile.");
        }

        // Convert UserId claim to integer
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Content("Unable to resolve user profile.");
        }

        // Retrieve the logged-in user's record
        var user = _dbContext.Users
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return Content("User profile not found.");
        }

        // Display user information
        UserDto.Username = user.UserName;
        UserDto.FirstName = user.FirstName;
        UserDto.LastName = user.LastName;
        UserDto.DateOfBirth = user.DateOfBirth;

        // Get profile image
        var avatarPath = Path.Combine(
            _environment.WebRootPath,
            "users",
            $"{user.Id}.png");

        if (System.IO.File.Exists(avatarPath))
        {
            UserDto.ProfileImage = $"/users/{user.Id}.png";
        }
        else
        {
            UserDto.ProfileImage = "/users/default.png";
        }

        return Page();
    }
}

public class UserDto
{
    public string? Username { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    public string? ProfileImage { get; set; }
}