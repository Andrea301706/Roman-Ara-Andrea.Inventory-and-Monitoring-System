using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Models;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly InventorySystemDbContext _context;

        public DetailsModel(InventorySystemDbContext context)
        {
            _context = context;
        }

        public Product Product { get; set; } = new Product();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;

            return Page();
        }
    }
}