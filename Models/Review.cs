using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MiniWebApplication.Models
{
    public class Review
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("ProductId")]
        public int ProductId { get; set; }

        [JsonProperty("UserName")]
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        [JsonProperty("Rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        [JsonProperty("Comment")]
        public string Comment { get; set; }

        [JsonProperty("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Constructor to initialize properties
        public Review()
        {
            UserName = string.Empty;
            Comment = string.Empty;
            Timestamp = DateTime.UtcNow; 
        }
    }
}
