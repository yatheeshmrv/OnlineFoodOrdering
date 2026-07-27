using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderAPI.Models
{
    // Represents a registered user in the application.
    // IdentityUser already provides:
    // Id, UserName, Email, PhoneNumber, PasswordHash and security fields.
    public class ApplicationUser : IdentityUser
    {
        // Additional property required by our food-ordering application.
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}