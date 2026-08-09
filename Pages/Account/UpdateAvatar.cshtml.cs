using System.Diagnostics;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using SixLabors.ImageSharp;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Microsoft.AspNetCore.Hosting;
public class UpdateAvatar : PageModel
{  

    private readonly InventorySystemDbContext _dbContext;

    private readonly IWebHostEnvironment _environment;

    public UpdateAvatar(
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null)
        {
            return NotFound();
        }

        UserAvatarDto.Username = user.UserName;
        UserAvatarDto.FirstName = user.FirstName;
        UserAvatarDto.LastName = user.LastName;

        var avatarDiskPath = Path.Combine(_environment.WebRootPath, "users", $"{user.Id}.png");
        if (System.IO.File.Exists(avatarDiskPath))
        {
            UserAvatarDto.ProfileImage = $"/users/{user.Id}.png";
        }
        else
        {
            UserAvatarDto.ProfileImage = "/users/default.png";// Path to the default profile image
        }
        return Page();
    }

    public IActionResult OnPost()
    {
        if(!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null)
        {
            return NotFound();
        }

        if (UserAvatarDto.AvatarImage != null && UserAvatarDto.AvatarImage.Length > 0)
        {
            Image image;
            try
            {
                using (var uploadStream = UserAvatarDto.AvatarImage.OpenReadStream())
                {
                    image = Image.Load(uploadStream);
                }
            }
            catch (UnknownImageFormatException)
            {
                ModelState.AddModelError("UserAvatarDto.AvatarImage", "Only image files are allowed.");
                return Page();
            }
            catch (InvalidImageContentException)
            {
                ModelState.AddModelError("UserAvatarDto.AvatarImage", "Only image files are allowed.");
                return Page();
            }

            using (image)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "users");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // FileMode.Create overwrites the user's existing avatar, if any.
                var filePath = Path.Combine(uploadsFolder, $"{user.Id}.png");
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.SaveAsPng(fileStream);
                }
            }
        }

        _dbContext.SaveChanges();
        return RedirectToPage("/Account/Profile");
    }

}

public class UserAvatarDto
{    
    public string? Username { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
    public string? ProfileImage { get; set; }

    [Required(ErrorMessage = "Avatar Image is required.")]
    public IFormFile? AvatarImage { get; set; } 

}