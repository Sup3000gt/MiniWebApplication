using MiniWebApplication.Models;

namespace MiniWebApplication.ViewModels
{
    public class CheckoutViewModel
    {
        public List<PaymentCard> PaymentCards { get; set; }
        public List<ShoppingCartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

}
