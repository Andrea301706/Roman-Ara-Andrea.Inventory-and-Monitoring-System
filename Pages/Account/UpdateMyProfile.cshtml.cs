using System.Diagnostics;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;
public class UpdateProfile : PageModel
{
    private readonly InventorySystemDbContext _dbContext;

    private readonly IWebHostEnvironment _environment;

    public UpdateProfile(
        InventorySystemDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [BindProperty]
    public UserUpdateDto UserUpdateDto { get; set; } = new UserUpdateDto();
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

        UserUpdateDto.Username = user.UserName;
        UserUpdateDto.FirstName = user.FirstName;
        UserUpdateDto.LastName = user.LastName;
        UserUpdateDto.DateOfBirth = user.DateOfBirth;

        var avatarDiskPath = Path.Combine(_environment.WebRootPath, "users", $"{user.Id}.png");
        if (System.IO.File.Exists(avatarDiskPath))
        {
            UserUpdateDto.ProfileImage = $"/users/{user.Id}.png";
        }
        else
        {
            UserUpdateDto.ProfileImage = "/users/default.png";// Path to the default profile image
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
 
        user.FirstName = UserUpdateDto.FirstName;
        user.LastName = UserUpdateDto.LastName;
        user.DateOfBirth = (DateTime)UserUpdateDto.DateOfBirth!;

        _dbContext.SaveChanges();
       return RedirectToPage("/Account/MyProfile");
    }

}

public class UserUpdateDto
{    
    public string? Username { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime? DateOfBirth { get; set; }
    public string? ProfileImage { get; set; }

}