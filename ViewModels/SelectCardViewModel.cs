using MiniWebApplication.Models;

namespace MiniWebApplication.ViewModels
{
    public class SelectCardViewModel
    {
        public IEnumerable<PaymentCard> PaymentCards { get; set; }
        public List<Product> RecommendedProducts { get; set; }
    }

}
