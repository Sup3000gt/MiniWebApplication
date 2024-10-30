using System.ComponentModel.DataAnnotations;

namespace MiniWebApplication.ViewModels
{
    public class EmailViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string ConfirmationLink { get; set; }

        // Constructor to initialize properties
        public EmailViewModel()
        {
            FirstName = string.Empty;
            ConfirmationLink = string.Empty;
        }
    }
}
