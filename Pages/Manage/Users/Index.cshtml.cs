using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Manage.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly InventorySystemDbContext _dbContext;

    public IndexModel(InventorySystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<UserDisplay> Users { get; set; } = new List<UserDisplay>();

    public async Task OnGetAsync()
    {
        Users = await (
            from user in _dbContext.Users
            join loginInfo in _dbContext.UserLoginInfos
                on user.Id equals loginInfo.UserId
            where loginInfo.Key == "Role"
            select new UserDisplay
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Role = loginInfo.Value
            }
        ).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync(
        int userId,
        string role)
    {
        // Only allow these two roles
        if (role != "Admin" && role != "User")
        {
            return RedirectToPage();
        }

        var roleRecord = await _dbContext.UserLoginInfos
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Key == "Role");

        if (roleRecord == null)
        {
            roleRecord = new UserLogininfo
            {
                UserId = userId,
                Key = "Role",
                Value = role
            };

            _dbContext.UserLoginInfos.Add(roleRecord);
        }
        else
        {
            roleRecord.Value = role;
        }

        await _dbContext.SaveChangesAsync();

        return RedirectToPage();
    }

    public class UserDisplay
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}