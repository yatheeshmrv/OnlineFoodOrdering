using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        // Navigation property for the user's shopping cart.
        // The database relationship will ensure that one user
        // can have only one cart.
        //
        // This property is nullable because the cart will be created
        // only when the customer first views it or adds an item.
        //
        // JsonIgnore prevents cart information from being returned
        // whenever an ApplicationUser object is serialized.
        [JsonIgnore]
        public Cart? Cart { get; set; }

        // Navigation property for the customer's saved delivery
        // addresses.
        //
        // One customer can save multiple addresses such as
        // Home, Work or Other.
        //
        // JsonIgnore prevents address information from being returned
        // whenever an ApplicationUser object is serialized.
        [JsonIgnore]
        public List<UserAddress> UserAddresses { get; set; } =
            new List<UserAddress>();
    }
}