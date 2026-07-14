using System.ComponentModel.DataAnnotations;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Domain
{
    public class UserLoginInfo
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;
    }
}