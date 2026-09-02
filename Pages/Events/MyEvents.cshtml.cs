using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Events;

[Authorize(Roles = "User")]
public class MyEventsModel : PageModel
{
    public void OnGet()
    {
    }
}