using System.ComponentModel.DataAnnotations;

namespace MiniWebApplication.Models
{
    public class UserProfile
    {
        public int UserProfileId { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } // Navigation property to link to the User table

        [Required]
        [StringLength(50)]
        public string Tag { get; set; } // Each tag associated with a user's orders

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Tag count must be at least 1.")]
        public int TagCount { get; set; } = 1; // Incremented with each order that includes this tag
    }

}
