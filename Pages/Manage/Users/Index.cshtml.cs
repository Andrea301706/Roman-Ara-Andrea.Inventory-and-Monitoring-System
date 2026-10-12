using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Manage.Users;

public class IndexModel : PageModel
{
    private readonly InventorySystemDbContext _dbContext;

    public IndexModel(InventorySystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool IsAuthenticated =>
        User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin =>
        User.IsInRole("Admin");

    public string CurrentRole =>
        User.Claims
            .Where(c =>
                c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .FirstOrDefault() ?? "NO ROLE";

    public string CurrentUserName =>
        User.Identity?.Name ?? "NO USERNAME";

    public List<UserDisplay> Users { get; set; } = new();

    public string Keyword { get; set; } = string.Empty;

    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }

    public int TotalRecords { get; set; }

    public string SortBy { get; set; } = "username";

    public string SortOrder { get; set; } = "asc";


    // =========================================================
    // LOAD USERS
    // =========================================================

    public async Task OnGetAsync(
        int pageIndex = 1,
        int pageSize = 10,
        string? sortBy = "username",
        string? sortOrder = "asc",
        string? keyword = "")
    {
        PageIndex = pageIndex < 1 ? 1 : pageIndex;

        PageSize = pageSize <= 0 ? 10 : pageSize;

        Keyword = keyword?.Trim().ToLower() ?? string.Empty;

        SortBy = sortBy?.Trim().ToLower() ?? "username";

        SortOrder = sortOrder?.Trim().ToLower() ?? "asc";


        IQueryable<User> query = _dbContext.Users;


        // =====================================================
        // SEARCH BY USERNAME
        // =====================================================

        if (!string.IsNullOrEmpty(Keyword))
        {
            query = query.Where(user =>
                user.UserName != null &&
                user.UserName.ToLower().Contains(Keyword));
        }


        // =====================================================
        // TOTAL RECORDS
        // =====================================================

        TotalRecords = await query.CountAsync();


        // =====================================================
        // TOTAL PAGES
        // =====================================================

        TotalPages = (int)Math.Ceiling(
            TotalRecords / (double)PageSize);


        if (TotalPages > 0 && PageIndex > TotalPages)
        {
            PageIndex = TotalPages;
        }


        // =====================================================
        // SORT USERNAME
        // =====================================================

        if (SortBy == "username" && SortOrder == "desc")
        {
            query = query.OrderByDescending(user => user.UserName);
        }
        else
        {
            query = query.OrderBy(user => user.UserName);

            SortBy = "username";
            SortOrder = "asc";
        }


        // =====================================================
        // PAGINATION
        // =====================================================

        var skip = (PageIndex - 1) * PageSize;

        query = query
            .Skip(skip)
            .Take(PageSize);


        // =====================================================
        // GET USERS
        // =====================================================

        var users = await query.ToListAsync();


        // =====================================================
        // GET USER IDS
        // =====================================================

        var userIds = users
            .Select(user => user.Id)
            .ToList();


        // =====================================================
        // GET USER ROLES
        // =====================================================

        var roleRecords = await _dbContext.UserLoginInfos
            .Where(x =>
                userIds.Contains(x.UserId) &&
                x.Key.ToLower() == "role")
            .ToListAsync();


        // =====================================================
        // PREPARE DISPLAY DATA
        // =====================================================

        Users = users.Select(user =>
        {
            var role = roleRecords
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Value)
                .FirstOrDefault();

            return new UserDisplay
            {
                UserId = user.Id,

                UserName = user.UserName ?? string.Empty,

                FirstName = user.FirstName ?? string.Empty,

                LastName = user.LastName ?? string.Empty,

                DateOfBirth = user.DateOfBirth,

                Role = role ?? "User"
            };

        }).ToList();
    }


    // =========================================================
    // CHANGE USER ROLE
    // =========================================================

    public async Task<IActionResult> OnPostAsync(
        int userId,
        string role)
    {
        if (role != "Admin" && role != "User")
        {
            return RedirectToPage();
        }


        var roleRecord = await _dbContext.UserLoginInfos
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Key.ToLower() == "role");


        if (roleRecord == null)
        {
            roleRecord = new UserLogininfo
            {
                UserId = userId,

                Key = "role",

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


    // =========================================================
    // DELETE USER
    // =========================================================

    public async Task<IActionResult> OnPostDeleteAsync(int userId)
    {
        // Find the user
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId);


        // If user does not exist
        if (user == null)
        {
            return RedirectToPage();
        }


        // =====================================================
        // REMOVE USER LOGIN INFORMATION
        // =====================================================

        var loginInfos = await _dbContext.UserLoginInfos
            .Where(x => x.UserId == userId)
            .ToListAsync();


        if (loginInfos.Any())
        {
            _dbContext.UserLoginInfos.RemoveRange(loginInfos);
        }


        // =====================================================
        // REMOVE USER
        // =====================================================

        _dbContext.Users.Remove(user);


        // =====================================================
        // SAVE CHANGES
        // =====================================================

        await _dbContext.SaveChangesAsync();


        // Return to Manage Users page
        return RedirectToPage();
    }


    // =========================================================
    // USER DISPLAY MODEL
    // =========================================================

    public class UserDisplay
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}