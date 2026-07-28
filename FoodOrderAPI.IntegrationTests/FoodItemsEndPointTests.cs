using System.Net;
using Xunit;

namespace FoodOrderAPI.IntegrationTests
{
    public class FoodItemsEndpointTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public FoodItemsEndpointTests(
            CustomWebApplicationFactory factory)
        {
            // Creates an HTTP client connected to the test API.
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetFoodItems_WhenItemsExist_ReturnsOkWithItems()
        {
            // Act
            var response =
                await _client.GetAsync("/api/FoodItems");

            var responseBody =
                await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            Assert.Contains(
                "Paneer Fried Rice",
                responseBody);

            Assert.Contains(
                "Healthy Meals",
                responseBody);
        }
    }
}