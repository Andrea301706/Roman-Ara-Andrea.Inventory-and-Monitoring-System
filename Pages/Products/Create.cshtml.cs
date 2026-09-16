using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Models;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly InventorySystemDbContext _context;

        public CreateModel(InventorySystemDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = new Product();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product added successfully!";

            return RedirectToPage("./Index");
        }
    }
}