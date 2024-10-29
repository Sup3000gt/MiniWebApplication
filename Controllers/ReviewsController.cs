using Microsoft.AspNetCore.Mvc;
using MiniWebApplication.Models;
using MiniWebApplication.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ReviewsController : Controller
{
    private readonly CosmosDbService _cosmosDbService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(CosmosDbService cosmosDbService, ILogger<ReviewsController> logger)
    {
        _cosmosDbService = cosmosDbService;
        _logger = logger;
    }

    // Add a new review
    [HttpPost]
    public async Task<IActionResult> AddReview(Review review)
    {
        if (ModelState.IsValid)
        {
            await _cosmosDbService.AddReviewAsync(review);
            return RedirectToAction("Details", "Products", new { id = review.ProductId });
        }
        return View(review);
    }


    // Get all reviews for a product and calculate average rating
    [HttpGet]
    public async Task<IActionResult> GetReviews(int productId, int page = 1, int pageSize = 5)
    {
        var reviews = await _cosmosDbService.GetReviewsByProductIdAsync(productId);

        var orderedReviews = reviews.OrderByDescending(r => r.Timestamp).ToList();

        _logger.LogInformation("ReviewsController loaded successfully");
        _logger.LogInformation($"First review: {orderedReviews.FirstOrDefault()?.Timestamp}");

        // Reverse page logic: Start from newest first.
        var totalPages = (int)Math.Ceiling((double)orderedReviews.Count / pageSize);
        var reversedPage = totalPages - page + 1; // Adjust page index to start from end.

        var paginatedReviews = orderedReviews
            .Skip((reversedPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new ProductReviewsViewModel
        {
            Reviews = paginatedReviews,
            AverageRating = orderedReviews.Any() ? orderedReviews.Average(r => r.Rating) : 0,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return PartialView("_ReviewsList", viewModel);
    }


}
