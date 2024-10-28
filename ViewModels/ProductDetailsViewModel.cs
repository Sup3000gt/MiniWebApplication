namespace MiniWebApplication.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; } = 0.0;

        // Add these properties for pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
