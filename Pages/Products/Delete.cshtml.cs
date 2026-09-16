using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Models;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly InventorySystemDbContext _context;

        public DeleteModel(InventorySystemDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (Product.Id == 0)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == Product.Id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}