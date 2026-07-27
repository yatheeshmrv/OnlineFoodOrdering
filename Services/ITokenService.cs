using FoodOrderAPI.Models;

namespace FoodOrderAPI.Services
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user);
    }
}