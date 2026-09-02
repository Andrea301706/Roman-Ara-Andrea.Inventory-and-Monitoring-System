using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    public void OnGet()
    {
    }
}