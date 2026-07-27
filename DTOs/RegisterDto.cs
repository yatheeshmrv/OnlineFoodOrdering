namespace FoodOrderAPI.DTOs
{
    // Contains the information required to register a new user.
    // Validation is handled by RegisterDtoValidator.
    public class RegisterDto
    {
        // Full name of the registering user.
        public string FullName { get; set; } = string.Empty;

        // Email address used for login and communication.
        public string Email { get; set; } = string.Empty;

        // Customer's registered phone number.
        public string PhoneNumber { get; set; } = string.Empty;

        // Password used to secure the account.
        public string Password { get; set; } = string.Empty;

        // Must match the Password value.
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}