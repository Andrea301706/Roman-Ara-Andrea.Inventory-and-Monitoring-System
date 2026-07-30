using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages.Account
{
    public class RegisterModel : PageModel
    {
        public class RegisterInputModel
        {
            public string UserName { get; set; } = "";

            public string FirstName { get; set; } = "";

            public string LastName { get; set; } = "";

            public DateTime DateOfBirth { get; set; }

            public string Password { get; set; } = "";

            public string ConfirmPassword { get; set; } = "";
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Step 2 - Required Fields Validation
            if (string.IsNullOrWhiteSpace(Input.UserName) ||
                string.IsNullOrWhiteSpace(Input.FirstName) ||
                string.IsNullOrWhiteSpace(Input.LastName) ||
                string.IsNullOrWhiteSpace(Input.Password) ||
                string.IsNullOrWhiteSpace(Input.ConfirmPassword))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Other validation steps will go here

            return RedirectToPage("/Account/Login");
        }
    }
}