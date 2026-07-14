using System.ComponentModel.DataAnnotations;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string ProductCode { get; set; } = "";

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public decimal UnitPrice { get; set; }

        public int CurrentStock { get; set; }

        public int ReorderLevel { get; set; }
    }
}