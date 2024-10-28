namespace MiniWebApplication.Models
{
    public class ProductReviewsViewModel
    {
        public List<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; } = 0.0;

        // New properties for pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
