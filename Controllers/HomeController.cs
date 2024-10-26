using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using System.Diagnostics;

namespace MiniWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: Home/Search
        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            const int PageSize = 9;

            ViewData["IsSearchExecuted"] = true;
            ViewData["SearchQuery"] = query; // Pass the query string

            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(query.ToLower()) ||
                    p.Description.ToLower().Contains(query.ToLower()));
            }

            var totalItems = await productsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var products = await productsQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View("Index", products);
        }

    }
}
