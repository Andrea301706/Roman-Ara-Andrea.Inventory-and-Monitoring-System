using System.ComponentModel.DataAnnotations;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.DTOs
{
    public class ChangePasswordDto
    {
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current Password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        public string ProfileImage { get; set; } = string.Empty;
    }
}