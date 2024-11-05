using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using Microsoft.Extensions.Logging;

namespace MiniWebApplication.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(ApplicationDbContext context, ILogger<RecommendationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Private method to fetch popular products
        private async Task<List<Product>> GetTopPopularProductsAsync(int maxCount = 5)
        {
            _logger.LogInformation("Fetching top popular products");
            var popularProducts = await _context.ProductPopularities
                .OrderByDescending(pp => pp.SalesCount)
                .Take(maxCount)
                .Include(pp => pp.Product) 
                .Select(pp => pp.Product)
                .ToListAsync();

            return popularProducts;
        }

        // Action to return the partial view
        [HttpGet]
        public async Task<IActionResult> GetTopPopularProducts(int maxCount = 5)
        {
            var popularProducts = await _context.ProductPopularities
                .OrderByDescending(pp => pp.SalesCount)
                .Take(maxCount)
                .Include(pp => pp.Product) // Ensure navigation property is included
                .Select(pp => pp.Product)
                .ToListAsync();

            return PartialView("_PopularProducts", popularProducts);
        }

        // Fetch top N recommended products for a user
        private async Task<List<Product>> GetTopRecommendedProductsForUser(int userId, int maxCount = 3)
        {
            _logger.LogInformation("Fetching top recommended products for userId: {UserId}", userId);

            var topUserTag = await _context.UserProfiles
                .Where(up => up.UserId == userId)
                .OrderByDescending(up => up.TagCount)
                .Select(up => up.Tag)
                .FirstOrDefaultAsync();

            if (topUserTag == null)
            {
                _logger.LogWarning("No tags found for userId: {UserId}", userId);
                return new List<Product>();
            }

            var productsWithTag = await _context.Products
                .Where(p => p.Tags.Contains(topUserTag))
                .Include(p => p.ProductPopularities)
                .ToListAsync();

            if (!productsWithTag.Any())
            {
                _logger.LogWarning("No products found for tag: {Tag} for userId: {UserId}", topUserTag, userId);
                return new List<Product>();
            }

            var recommendedProducts = productsWithTag
                .OrderByDescending(p => p.ProductPopularities.Sum(pp => pp.SalesCount))
                .Take(maxCount)
                .ToList();

            _logger.LogInformation("Found {Count} recommended products for userId: {UserId}", recommendedProducts.Count, userId);

            return recommendedProducts;
        }


        [HttpGet]
        public async Task<IActionResult> RecommendProduct(int userId)
        {
            _logger.LogInformation("RecommendProduct called for userId: {UserId}", userId);

            var recommendedProducts = await GetTopRecommendedProductsForUser(userId);

            if (recommendedProducts == null || !recommendedProducts.Any())
            {
                return NoContent();
            }

            _logger.LogInformation("Returning {Count} recommended products for userId: {UserId}", recommendedProducts.Count, userId);

            return PartialView("_RecommendedProduct", recommendedProducts);
        }
    }
}
