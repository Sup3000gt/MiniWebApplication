using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniWebApplication.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _protector;

        public PaymentController(ApplicationDbContext context, IDataProtectionProvider provider)
        {
            _context = context;
            _protector = provider.CreateProtector("PaymentCardProtector");
        }

        // GET: Payment/AddCard
        public IActionResult AddCard()
        {
            return View();
        }

        // POST: Payment/AddCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCard(PaymentCard card)
        {
            ModelState.Remove(nameof(card.LastFourDigits));

            if (ModelState.IsValid)
            {
                try
                {
                    card.UserId = GetUserId();

                    // Capture the last four digits before encryption
                    card.LastFourDigits = card.CardNumber.Substring(card.CardNumber.Length - 4);

                    // Encrypt sensitive fields
                    card.CardNumber = _protector.Protect(card.CardNumber);
                    card.CVV = _protector.Protect(card.CVV);

                    _context.PaymentCards.Add(card);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("SelectCard");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the card.");
                }
            }
            else
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View(card);
        }


        // GET: Payment/SelectCard
        public async Task<IActionResult> SelectCard()
        {
            int userId = GetUserId();

            // Fetch only active cards for this user
            var paymentCards = await _context.PaymentCards
                .Where(c => c.UserId == userId && c.IsActive)
                .ToListAsync();

            if (!paymentCards.Any())
            {
                TempData["Info"] = "You have no active payment cards. Please add a payment method.";
            }

            return View(paymentCards);
        }


        // POST: Payment/SelectCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectCard(int selectedCardId)
        {
            return RedirectToAction("PlaceOrder", "ShoppingCart", new { selectedCardId });
        }

        // GET: Payment/DeleteCard/5
        public async Task<IActionResult> DeleteCard(int id)
        {
            var paymentCard = await _context.PaymentCards.FindAsync(id);

            if (paymentCard == null || paymentCard.UserId != GetUserId())
            {
                TempData["Error"] = "Payment card not found.";
                return RedirectToAction("SelectCard");
            }

            return View(paymentCard);
        }
        // POST: Payment/DeleteCardConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCardConfirmed(int id)
        {
            var paymentCard = await _context.PaymentCards.FindAsync(id);

            if (paymentCard == null || paymentCard.UserId != GetUserId())
            {
                TempData["Error"] = "Payment card not found.";
                return RedirectToAction("SelectCard");
            }

            paymentCard.IsActive = false; // Mark the card as inactive
            _context.PaymentCards.Update(paymentCard);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment card deactivated successfully.";
            return RedirectToAction("SelectCard");
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new Exception("User is not authenticated.");
            }
            return int.Parse(userIdClaim);
        }
    }
}
