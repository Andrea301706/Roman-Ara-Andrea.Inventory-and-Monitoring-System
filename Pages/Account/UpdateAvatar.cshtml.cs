using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using System.Security.Claims;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account;

[Authorize]
public class UpdateAvatarModel : PageModel
{
private readonly InventorySystemDbContext _dbContext;
private readonly IWebHostEnvironment _environment;

public UpdateAvatarModel(
    InventorySystemDbContext dbContext,
    IWebHostEnvironment environment)
{
    _dbContext = dbContext;
    _environment = environment;
}

[BindProperty]
public UserAvatarDto UserAvatarDto { get; set; } = new UserAvatarDto();

public IActionResult OnGet()
{
    // Get UserId from logged-in user's claims
    var userIdClaim =
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Redirect to Login if UserId claim is missing
    if (string.IsNullOrWhiteSpace(userIdClaim))
    {
        return RedirectToPage("/Account/Login");
    }

    // Convert UserId claim to integer
    if (!int.TryParse(userIdClaim, out int userId))
    {
        return RedirectToPage("/Account/Login");
    }

    // Retrieve logged-in user
    var user = _dbContext.Users
        .FirstOrDefault(u => u.Id == userId);

    // User record not found
    if (user == null)
    {
        return NotFound();
    }

    // Load user information
    UserAvatarDto.Username = user.UserName;
    UserAvatarDto.FirstName = user.FirstName;
    UserAvatarDto.LastName = user.LastName;
    UserAvatarDto.Email = user.Email;

    // Check existing avatar
    var avatarPath = Path.Combine(
        _environment.WebRootPath,
        "users",
        $"{user.Id}.png");

    if (System.IO.File.Exists(avatarPath))
    {
        UserAvatarDto.ProfileImage =
            $"/users/{user.Id}.png";
    }
    else
    {
        UserAvatarDto.ProfileImage =
            "/users/default.png";
    }

    return Page();
}

public async Task<IActionResult> OnPostAsync()
{
    // Get UserId from logged-in user's claims
    var userIdClaim =
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Redirect to Login if UserId claim is missing
    if (string.IsNullOrWhiteSpace(userIdClaim))
    {
        return RedirectToPage("/Account/Login");
    }

    // Convert UserId claim to integer
    if (!int.TryParse(userIdClaim, out int userId))
    {
        return RedirectToPage("/Account/Login");
    }

    // Retrieve logged-in user
    var user = _dbContext.Users
        .FirstOrDefault(u => u.Id == userId);

    if (user == null)
    {
        return NotFound();
    }

    // Validate that an avatar was provided
    if (UserAvatarDto.AvatarImage == null ||
        UserAvatarDto.AvatarImage.Length == 0)
    {
        ModelState.AddModelError(
            "UserAvatarDto.AvatarImage",
            "Avatar Image is required.");

        LoadUserInformation(user);

        return Page();
    }

    // Validate that uploaded file is a real image
    try
    {
        using var stream =
            UserAvatarDto.AvatarImage.OpenReadStream();

        using var image =
            await Image.LoadAsync(stream);
    }
    catch
    {
        ModelState.AddModelError(
            "UserAvatarDto.AvatarImage",
            "Only image files are allowed.");

        LoadUserInformation(user);

        return Page();
    }

    // Create wwwroot/users folder if it does not exist
    var usersFolder = Path.Combine(
        _environment.WebRootPath,
        "users");

    if (!Directory.Exists(usersFolder))
    {
        Directory.CreateDirectory(usersFolder);
    }

    // Save avatar using UserId as filename
    var avatarPath = Path.Combine(
        usersFolder,
        $"{user.Id}.png");

    // Load uploaded image
    using var uploadStream =
        UserAvatarDto.AvatarImage.OpenReadStream();

    using var uploadedImage =
        await Image.LoadAsync(uploadStream);

    // Save image as PNG
    // Existing avatar is automatically replaced.
    await uploadedImage.SaveAsPngAsync(
        avatarPath,
        new PngEncoder());

    // Redirect to Profile page
    return RedirectToPage("/Account/Profile");
}

private void LoadUserInformation(Domain.User user)
{
    UserAvatarDto.Username = user.UserName;
    UserAvatarDto.FirstName = user.FirstName;
    UserAvatarDto.LastName = user.LastName;
    UserAvatarDto.Email = user.Email;

    var avatarPath = Path.Combine(
        _environment.WebRootPath,
        "users",
        $"{user.Id}.png");

    if (System.IO.File.Exists(avatarPath))
    {
        UserAvatarDto.ProfileImage =
            $"/users/{user.Id}.png";
    }
    else
    {
        UserAvatarDto.ProfileImage =
            "/users/default.png";
    }
}


}

public class UserAvatarDto
{
public string? Username { get; set; }


public string? FirstName { get; set; }

public string? LastName { get; set; }

public string? Email { get; set; }

public string? ProfileImage { get; set; }

public IFormFile? AvatarImage { get; set; }

}
