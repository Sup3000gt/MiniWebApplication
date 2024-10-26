using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        // POST: ShoppingCart/PlaceOrder
        // POST: ShoppingCart/PlaceOrder
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            int userId = GetUserId();

            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index");
            }

            // Calculate the total amount
            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);

            // Create new order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to get the Order ID

            // Create order details
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price  // Save the product price at the time of the order
                };
                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            // Clear the cart
            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("Index", "Orders");
        }

    }
}
