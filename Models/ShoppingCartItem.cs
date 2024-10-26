using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniWebApplication.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int ShoppingCartItemId { get; set; }

        [Required]
        public int UserId { get; set; } 

        [Required]
        [ForeignKey(nameof(Product))] // Use nameof() for maintainability
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        // Navigation property to the Product entity
        public Product Product { get; set; }
    }
}
