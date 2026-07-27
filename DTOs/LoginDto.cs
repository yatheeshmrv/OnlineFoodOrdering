namespace FoodOrderAPI.DTOs
{
    // Contains the credentials required to log in.
    public class LoginDto
    {
        // Email address entered by the user.
        public string Email { get; set; } = string.Empty;

        // Password entered by the user.
        public string Password { get; set; } = string.Empty;
    }
}