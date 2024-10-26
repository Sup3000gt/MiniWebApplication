using System.ComponentModel.DataAnnotations;
using System;

namespace MiniWebApplication.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // EmailConfirmed property
        [Required]
        public bool EmailConfirmed { get; set; } = false;

        // EmailConfirmationToken
        public string? EmailConfirmationToken { get; set; }

        // EmailConfirmationTokenExpires property
        public DateTime EmailConfirmationTokenExpires { get; set; }

    }
}
