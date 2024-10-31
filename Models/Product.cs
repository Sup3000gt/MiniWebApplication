using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniWebApplication.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } // Internal use only

        [NotMapped] // This ensures the field is not mapped to the database
        public IFormFile ImageFile { get; set; } // For file upload

        // Constructor to initialize properties
        public Product()
        {
            Name = string.Empty;
            Description = string.Empty;
            ImageUrl = string.Empty;
            // ImageFile can remain null if appropriate
        }
    }
}
