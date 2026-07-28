using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace FoodOrderAPI.IntegrationTests
{
    public class AuthorizationEndpointTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthorizationEndpointTests(
            CustomWebApplicationFactory factory)
        {
            // Creates an HTTP client connected to the test API.
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllOrders_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var response =
                await _client.GetAsync("/api/Order");

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task GetAllOrders_WithCustomerToken_ReturnsForbidden()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/Order");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            // Act
            var response =
                await _client.SendAsync(request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task GetAllOrders_WithAdminToken_ReturnsOk()
        {
            // Arrange
            var adminToken =
                await CreateAdminTokenAsync();

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/Order");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var response =
                await _client.SendAsync(request);

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }

        private async Task<string> CreateCustomerTokenAsync()
        {
            const string customerEmail =
                "authorization-customer@example.com";

            const string customerPassword =
                "Customer123";

            var registerRequest = new
            {
                FullName = "Authorization Test Customer",
                Email = customerEmail,
                PhoneNumber = "9876543212",
                Password = customerPassword,
                ConfirmPassword = customerPassword
            };

            var registerResponse =
                await _client.PostAsJsonAsync(
                    "/api/Auth/register",
                    registerRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                registerResponse.StatusCode);

            var loginRequest = new
            {
                Email = customerEmail,
                Password = customerPassword
            };

            var loginResponse =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    loginRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var responseBody =
                await loginResponse.Content
                    .ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            return jsonDocument.RootElement
                .GetProperty("token")
                .GetString()!;
        }

        private async Task<string> CreateAdminTokenAsync()
        {
            var loginRequest = new
            {
                Email = "integration-admin@test.local",
                Password = "IntegrationTest@12345"
            };

            var loginResponse =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    loginRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var responseBody =
                await loginResponse.Content
                    .ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            return jsonDocument.RootElement
                .GetProperty("token")
                .GetString()!;
        }


    }
}