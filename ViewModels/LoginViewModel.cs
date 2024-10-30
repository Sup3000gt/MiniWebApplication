using System.ComponentModel.DataAnnotations;

namespace MiniWebApplication.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Constructor to initialize properties
        public LoginViewModel()
        {
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}
