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

    // =====================================================
    // TEMPORARY AUTHENTICATION DEBUG
    // =====================================================

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


    // =====================================================
    // USERS
    // =====================================================

    public List<UserDisplay> Users { get; set; } = new();

    public string Keyword { get; set; } = string.Empty;

    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }

    public int TotalRecords { get; set; }

    public string SortBy { get; set; } = "username";

    public string SortOrder { get; set; } = "asc";


    public async Task OnGetAsync(
        int pageIndex = 1,
        int pageSize = 10,
        string? sortBy = "username",
        string? sortOrder = "asc",
        string? keyword = "")
    {
        // STEP 1: Set safe default values
        PageIndex = pageIndex < 1 ? 1 : pageIndex;
        PageSize = pageSize <= 0 ? 10 : pageSize;

        Keyword = keyword?.Trim().ToLower() ?? string.Empty;

        SortBy = sortBy?.Trim().ToLower() ?? "username";
        SortOrder = sortOrder?.Trim().ToLower() ?? "asc";


        // STEP 2: Start with Users table
        IQueryable<User> query = _dbContext.Users;


        // STEP 3: Search by username
        if (!string.IsNullOrEmpty(Keyword))
        {
            query = query.Where(user =>
                user.UserName != null &&
                user.UserName.ToLower().Contains(Keyword));
        }


        // STEP 4: Count filtered records
        TotalRecords = await query.CountAsync();


        // STEP 5: Calculate total pages
        TotalPages = (int)Math.Ceiling(
            TotalRecords / (double)PageSize);


        // STEP 6: Prevent invalid page numbers
        if (TotalPages > 0 && PageIndex > TotalPages)
        {
            PageIndex = TotalPages;
        }


        // STEP 7: Sort by username
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


        // STEP 8: Calculate Skip
        var skip = (PageIndex - 1) * PageSize;


        // STEP 9: Apply pagination
        query = query
            .Skip(skip)
            .Take(PageSize);


        // STEP 10: Retrieve users
        var users = await query.ToListAsync();


        // STEP 11: Retrieve roles
        var userIds = users
            .Select(user => user.Id)
            .ToList();

        var roleRecords = await _dbContext.UserLoginInfos
            .Where(x =>
                userIds.Contains(x.UserId) &&
                x.Key.ToLower() == "role")
            .ToListAsync();


        // STEP 12: Create display list
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


    public async Task<IActionResult> OnPostAsync(
        int userId,
        string role)
    {
        // Only allow Admin or User
        if (role != "Admin" && role != "User")
        {
            return RedirectToPage();
        }


        // Database uses Key = "role"
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