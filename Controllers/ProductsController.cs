using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using MiniWebApplication.Services;

namespace MiniWebApplication.Controllers
{
    public class ProductsController : Controller
    {
        // Injected services
        private readonly ApplicationDbContext _context;
        private readonly CosmosDbService _cosmosDbService;
        private const int PageSize = 9;

        // Constructor with dependency injection
        public ProductsController(ApplicationDbContext context, CosmosDbService cosmosDbService)
        {
            _context = context;
            _cosmosDbService = cosmosDbService;
        }

        // GET: Products
        public IActionResult Index(int page = 1)
        {
            var products = _context.Products
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)_context.Products.Count() / PageSize);

            return View(products);
        }

        // GET: Products/Details/
        public async Task<IActionResult> Details(int id)
        {
            // Fetch the product from the SQL database
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Retrieve reviews from Cosmos DB
            List<Review> reviews;
            try
            {
                reviews = await _cosmosDbService.GetReviewsByProductIdAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching reviews: {ex.Message}");
                reviews = new List<Review>(); // Fallback to an empty list
            }

            // Calculate the average rating
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;


            // Pass the view model to the view
            return View(product);
        }

        // GET: Products/Create
        [Authorize] // Allows any authenticated user
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Allows any authenticated user to post
        public async Task<IActionResult> Create([Bind("Name,Description,Price,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
    }
}
