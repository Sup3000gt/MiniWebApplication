﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiniWebApplication.Models
{
    public class PaymentCard
    {
        [Key]
        public int CardId { get; set; }

        // Foreign key to User
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        // Card Details
        [Required]
        [Display(Name = "Card Number")]
        [CreditCard(ErrorMessage = "Invalid card number")]
        [StringLength(16, MinimumLength = 13, ErrorMessage = "Card number must be between 13 and 16 digits.")]
        public string CardNumber { get; set; }

        [Required]
        [Display(Name = "Expiration Date")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Invalid expiration date format (MM/YY).")]
        public string ExpirationDate { get; set; } // Format MM/YY

        [Required]
        [Display(Name = "CVV")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string CVV { get; set; }

        [Required]
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative.")]
        public decimal Balance { get; set; } // Available balance

        // Billing Address
        [Required]
        [Display(Name = "Billing Address Line 1")]
        [StringLength(100)]
        public string BillingAddressLine1 { get; set; }

        [Display(Name = "Billing Address Line 2")]
        [StringLength(100)]
        public string BillingAddressLine2 { get; set; }

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

        [Required]
        [Display(Name = "Country")]
        [StringLength(50)]
        public string BillingCountry { get; set; }

        // Constructor
        public PaymentCard()
        {
            CardNumber = string.Empty;
            ExpirationDate = string.Empty;
            CVV = string.Empty;
            BillingAddressLine1 = string.Empty;
            BillingAddressLine2 = string.Empty;
            BillingCity = string.Empty;
            BillingState = string.Empty;
            BillingPostalCode = string.Empty;
            BillingCountry = string.Empty;
        }
    }
}
