using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Models;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly InventorySystemDbContext _context;

        public EditModel(InventorySystemDbContext context)
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == Product.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.ProductCode = Product.ProductCode;
            existingProduct.Name = Product.Name;
            existingProduct.UnitPrice = Product.UnitPrice;
            existingProduct.CurrentStock = Product.CurrentStock;
            existingProduct.ReorderLevel = Product.ReorderLevel;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully!";

            return RedirectToPage("./Index");
        }
    }
}