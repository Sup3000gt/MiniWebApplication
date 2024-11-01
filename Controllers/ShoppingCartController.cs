﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MiniWebApplication.ViewModels;


namespace MiniWebApplication.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Safely get the user ID from claims
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new InvalidOperationException("User ID claim is missing.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                throw new InvalidOperationException("Invalid user ID format.");

            return userId;
        }

        // GET: ShoppingCart
        public IActionResult Index()
        {
            try
            {
                int userId = GetUserId();

                var cartItems = _context.ShoppingCartItems
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Product)
                    .ToList();

                // Check if there is a success message in TempData
                if (TempData["Success"] != null)
                {
                    ViewBag.SuccessMessage = TempData["Success"].ToString();
                }

                return View(cartItems);
            }
            catch (InvalidOperationException)
            {
                return RedirectToAction("Login", "Account");
            }
        }


        // POST: ShoppingCart/AddToCart/5
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            int userId = GetUserId();

            var cartItem = _context.ShoppingCartItems
                .SingleOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new ShoppingCartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // POST: ShoppingCart/RemoveFromCart/5
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            try
            {
                int userId = GetUserId();

                var cartItem = _context.ShoppingCartItems
                    .SingleOrDefault(c => c.UserId == userId && c.ProductId == productId);

                if (cartItem != null)
                {
                    _context.ShoppingCartItems.Remove(cartItem);
                    _context.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            catch (InvalidOperationException)
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: ShoppingCart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            int userId = GetUserId();

            var cartItem = _context.ShoppingCartItems
                .SingleOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // POST: ShoppingCart/Checkout
        [HttpPost]
        public IActionResult Checkout()
        {
            int userId = GetUserId();

            // Get available payment cards for the user
            var paymentCards = _context.PaymentCards.Where(c => c.UserId == userId).ToList();

            if (!paymentCards.Any())
            {
                TempData["Info"] = "You have no active payment cards. Please add a payment method.";
                return RedirectToAction("SelectCard", "Payment");
            }

            // Get cart items and calculate total amount
            var cartItems = _context.ShoppingCartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index");
            }

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);

            // Prepare a model or ViewBag data to send both paymentCards and totalAmount to the view
            var checkoutViewModel = new CheckoutViewModel
            {
                PaymentCards = paymentCards,
                CartItems = cartItems,
                TotalAmount = totalAmount
            };

            return View(checkoutViewModel);
        }

        // POST: ShoppingCart/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int selectedCardId)
        {
            // Redirect to SelectCard page without error if no card is selected (handled by front-end validation).
            if (selectedCardId == 0)
            {
                return RedirectToAction("SelectCard", "Payment");
            }

            int userId = GetUserId();

            var paymentCard = await _context.PaymentCards
                .Where(c => c.CardId == selectedCardId && c.UserId == userId && c.IsActive)
                .FirstOrDefaultAsync();

            if (paymentCard == null)
            {
                TempData["Error"] = "Selected payment method is invalid or inactive.";
                return RedirectToAction("SelectCard", "Payment");
            }

            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty! Order can not be create!";
                return RedirectToAction("SelectCard", "Payment"); 
            }


            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);

            if (paymentCard.Balance < totalAmount)
            {
                TempData["Error"] = "Purchase failed: Insufficient funds on the selected card. Please contact your bank or use a different card.";
                return RedirectToAction("SelectCard", "Payment");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Deduct the order amount from the payment card balance
                    paymentCard.Balance -= totalAmount;
                    _context.PaymentCards.Update(paymentCard);
                    await _context.SaveChangesAsync();

                    var order = new Order
                    {
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        PaymentCardId = paymentCard.CardId
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // Save to get the Order ID

                    foreach (var item in cartItems)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Product.Price 
                        };
                        _context.OrderDetails.Add(orderDetail);
                    }

                    await _context.SaveChangesAsync();

                    // Clear the user's shopping cart
                    _context.ShoppingCartItems.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();

                    // Commit the transaction after all changes are successfully saved
                    await transaction.CommitAsync();

                    TempData["Success"] = "Order placed successfully!";
                    return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
                }
                catch (Exception)
                {
                    // Roll back the transaction if any error occurs during the process
                    await transaction.RollbackAsync();
                    TempData["Error"] = "An error occurred while processing your order.";
                    return RedirectToAction("Index");
                }
            }
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            int userId = GetUserId();

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

    }
}
