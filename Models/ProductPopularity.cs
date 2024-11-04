using System.ComponentModel.DataAnnotations;

namespace MiniWebApplication.Models
{
    public class ProductPopularity
    {
        public int ProductPopularityId { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } // Navigation property

        [Required]
        public DateTime RecordDate { get; set; } // Date for tracking daily sales data

        public int SalesCount { get; set; } // Number of times this product was sold on RecordDate
    }
}
