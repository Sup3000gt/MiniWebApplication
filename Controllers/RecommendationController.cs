using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniWebApplication.Data;
using MiniWebApplication.Models;

namespace MiniWebApplication.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecommendationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Private method to fetch popular products
        private async Task<List<Product>> GetTopPopularProductsAsync(int maxCount = 5)
        {
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
    }
}
