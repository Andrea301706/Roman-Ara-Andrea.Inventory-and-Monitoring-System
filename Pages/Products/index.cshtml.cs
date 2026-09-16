using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Models;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly InventorySystemDbContext _context;

        public IndexModel(InventorySystemDbContext context)
        {
            _context = context;
        }

        public IList<Product> Products { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            Products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}