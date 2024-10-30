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

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
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

        // Constructor to initialize properties
        public User()
        {
            Username = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            // DateOfBirth can remain null if appropriate
        }

    }
}
