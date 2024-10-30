using System;
using System.ComponentModel.DataAnnotations;

namespace MiniWebApplication.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100)] // Adjust as needed
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Other properties
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [Phone]
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"\(\d{3}\)\s?\d{3}-\d{4}", ErrorMessage = "Phone number must be in the format (XXX) XXX-XXXX.")]
        public string PhoneNumber { get; set; }


        [DataType(DataType.Date)]

        [Display(Name = "Date of Birth")]
        [Required(ErrorMessage = "Date of Birth is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])/([0-2][1-9]|3[01])/([0-9]{4})$", ErrorMessage = "Invalid date format. Use mm/dd/yyyy.")]
        public string DateOfBirth { get; set; }

        // Constructor to initialize properties
        public RegisterViewModel()
        {
            Username = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            DateOfBirth = string.Empty;
        }
    }
}
