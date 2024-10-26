// Models/ProductReviewsViewModel.cs
using System.Collections.Generic;

namespace MiniWebApplication.Models
{
    public class ProductReviewsViewModel
    {
        public List<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; } = 0.0;
    }
}
