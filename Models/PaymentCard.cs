using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MiniWebApplication.Models
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null && DateTime.TryParseExact(value.ToString(), "MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expirationDate))
            {
                expirationDate = expirationDate.AddMonths(1).AddDays(-1); // Set expiration to the end of the month
                if (expirationDate >= DateTime.Now)
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult("The expiration date must be in the future.");
        }
    }
    public class PaymentCard
    {
        [Key]
        public int CardId { get; set; }

        // Foreign key to User
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever] // Prevent validation of this property
        public User User { get; set; }

        // Cardholder Name
        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; }

        // Card Details
        [Required]
        [Display(Name = "Card Number")]
        [StringLength(4000, ErrorMessage = "Card number length exceeded.")]
        public string CardNumber { get; set; }

        [Display(Name = "Last Four Digits")]
        [BindNever]
        public string LastFourDigits { get; set; }

        [Required]
        [Display(Name = "Expiration Date")]
        [RegularExpression(@"^(0[1-9]|1[0-2])/([0-9]{2})$", ErrorMessage = "Invalid expiration date format (MM/YY).")]
        [FutureDate(ErrorMessage = "The expiration date must be in the future.")] // Add custom validation attribute here
        public string ExpirationDate { get; set; } // Format MM/YY

        [Required]
        [Display(Name = "CVV")]
        [StringLength(4000, ErrorMessage = "CVV length exceeded.")]
        public string CVV { get; set; }


        [Required]
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative.")]
        public decimal Balance { get; set; } // Available balance

        // Billing Address
        [Required]
        [Display(Name = "Billing Address Line 1")]
        [StringLength(100)]
        public string BillingAddressLine1 { get; set; }

        [Required]
        [Display(Name = "City")]
        [StringLength(50)]
        public string BillingCity { get; set; }

        [Required]
        [Display(Name = "State/Province")]
        [StringLength(50)]
        public string BillingState { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        [StringLength(20)]
        public string BillingPostalCode { get; set; }
        public bool IsActive { get; set; } = true;

        // Constructor
        public PaymentCard()
        {
            CardNumber = string.Empty;
            ExpirationDate = string.Empty;
            CVV = string.Empty;
            BillingAddressLine1 = string.Empty;
            BillingCity = string.Empty;
            BillingState = string.Empty;
            BillingPostalCode = string.Empty;
        }
    }
}
