using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using MiniWebApplication.Services;
using MiniWebApplication.ViewModels;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace MiniWebApplication.Controllers
{
    public class ProductsController : Controller
    {
        // Injected services
        private readonly ApplicationDbContext _context;
        private readonly CosmosDbService _cosmosDbService;
        private readonly IConfiguration _configuration;
        private const int PageSize = 9;

        // Constructor with dependency injection
        public ProductsController(ApplicationDbContext context, CosmosDbService cosmosDbService, IConfiguration configuration)
        {
            _context = context;
            _cosmosDbService = cosmosDbService;
            _configuration = configuration;
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
        public async Task<IActionResult> Details(int id, int page = 1, int pageSize = 5)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var reviews = await _cosmosDbService.GetReviewsByProductIdAsync(id);
            var paginatedReviews = reviews
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)reviews.Count / pageSize);

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Reviews = paginatedReviews,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
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
        [Authorize]
        public async Task<IActionResult> Create(Product product)
        {
            try
            {
                // Ensure an image file is uploaded
                if (product.ImageFile == null || product.ImageFile.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Please upload an image.");
                    return View(product); // Return the form with error message
                }

                // Upload image to Blob Storage
                string containerName = "alanminiprojectimage";
                string blobUrl = await UploadImageToBlob(product.ImageFile, containerName);
                product.ImageUrl = blobUrl;

                var tagExtractor = new SimpleTagExtractor();
                // Generate tags based on the product description
                var tags = tagExtractor.ExtractTags(product.Description);

                // Convert list of tags to a comma-separated string
                product.Tags = string.Join(",", tags);


                // Save product to the database
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Redirect to Index only once
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(product); // Ensure no further response is attempted
            }
        }

        // Blob Upload Helper Method
        private async Task<string> UploadImageToBlob(IFormFile file, string containerName)
        {
            try
            {
                Console.WriteLine("Connecting to Blob Storage...");

                string blobConnectionString = _configuration.GetConnectionString("AzureBlobStorage");
                BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                string blobName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                Console.WriteLine($"Uploading file: {blobName}");

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                Console.WriteLine("File uploaded successfully.");
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload failed: {ex.Message}");
                throw; // Rethrow the exception to be handled by the controller
            }
        }
    }
}
