using Microsoft.AspNetCore.Authorization;
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
                TempData["Error"] = "Your cart is empty! Order cannot be created!";
                return RedirectToAction("SelectCard", "Payment");
            }

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);

            if (paymentCard.Balance < totalAmount)
            {
                TempData["Error"] = "Purchase failed: Insufficient funds on the selected card. Please contact your bank or use a different card.";
                return RedirectToAction("SelectCard", "Payment");
            }

            // Check if each product has sufficient inventory
            if (!await CheckInventoryAsync(cartItems))
            {
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

                    // Create the order
                    var order = new Order
                    {
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        PaymentCardId = paymentCard.CardId
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // Save to get the Order ID

                    // Process Order Details, Update Inventory, User Profile Tags, and Product Popularity
                    await ProcessOrderDetailsAsync(order, cartItems, userId);

                    // Clear the user's shopping cart
                    _context.ShoppingCartItems.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();

                    // Commit the transaction after all changes are successfully saved
                    await transaction.CommitAsync();

                    TempData["Success"] = "Order placed successfully!";
                    return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
                }
                catch (Exception ex)
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
  
        // Check Inventory
        private async Task<bool> CheckInventoryAsync(List<ShoppingCartItem> cartItems)
        {
            foreach (var item in cartItems)
            {
                if (item.Product.Inventory < item.Quantity)
                {
                    TempData["Error"] = $"Not enough inventory for {item.Product.Name}. Only {item.Product.Inventory} available.";
                    return false;
                }
            }
            return true;
        }
        // Process Order Details, Inventory Deduction
        private async Task ProcessOrderDetailsAsync(Order order, List<ShoppingCartItem> cartItems, int userId)
        {
            foreach (var item in cartItems)
            {
                // Deduct the inventory
                item.Product.Inventory -= item.Quantity;
                _context.Products.Update(item.Product);

                // Create order detail entry
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };
                _context.OrderDetails.Add(orderDetail);

                // Update UserProfile table for tags
                await UpdateUserProfileTagsAsync(userId, item.Product.Tags);
                // Update ProductPopularity table
                await UpdateProductPopularityAsync(item.ProductId, item.Quantity);
            }

            await _context.SaveChangesAsync();
        }

        // Update User Profile Tags
        private async Task UpdateUserProfileTagsAsync(int userId, string tags)
        {
            var tagList = tags.Split(',');

            foreach (var tag in tagList)
            {
                var userTag = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.Tag == tag);

                if (userTag != null)
                {
                    userTag.TagCount += 1; // Increment the tag count
                }
                else
                {
                    // Add new tag for user
                    _context.UserProfiles.Add(new UserProfile
                    {
                        UserId = userId,
                        Tag = tag,
                        TagCount = 1
                    });
                }
            }
        }
        //update ProductPopularity
        private async Task UpdateProductPopularityAsync(int productId, int quantity)
        {
            var productPopularity = await _context.ProductPopularities
                .FirstOrDefaultAsync(pp => pp.ProductId == productId);

            if (productPopularity != null)
            {
                productPopularity.SalesCount += quantity; // Increment by quantity ordered
            }
            else
            {
                // Add new product popularity entry
                _context.ProductPopularities.Add(new ProductPopularity
                {
                    ProductId = productId,
                    RecordDate = DateTime.Now, // Use current date, or adjust based on your requirements
                    SalesCount = quantity
                });
            }
        }



    }
}
