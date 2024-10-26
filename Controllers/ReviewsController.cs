using Microsoft.AspNetCore.Mvc;
using MiniWebApplication.Models;
using MiniWebApplication.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ReviewsController : Controller
{
    private readonly CosmosDbService _cosmosDbService;

    public ReviewsController(CosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
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
    public async Task<IActionResult> GetReviews(int productId)
    {
        var reviews = await _cosmosDbService.GetReviewsByProductIdAsync(productId);
        var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        var viewModel = new ProductReviewsViewModel
        {
            Reviews = reviews,
            AverageRating = averageRating
        };

        return PartialView("_ReviewsList", viewModel);
    }
}
