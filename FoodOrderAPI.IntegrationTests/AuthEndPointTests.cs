using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace FoodOrderAPI.IntegrationTests
{
    public class AuthEndpointTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthEndpointTests(
            CustomWebApplicationFactory factory)
        {
            // Creates an HTTP client connected to the test API.
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithValidAdminCredentials_ReturnsOkWithJwtToken()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "integration-admin@test.local",
                Password = "IntegrationTest@12345"
            };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

        var responseJson =
            jsonDocument.RootElement;

        // Confirms that the response contains a JWT.
        Assert.True(
            responseJson.TryGetProperty(
                    "token",
                    out var tokenElement));

        Assert.False(
            string.IsNullOrWhiteSpace(
                tokenElement.GetString()));

            // Confirms that the authentication scheme is Bearer.
            Assert.Equal(
                "Bearer",
                responseJson
                    .GetProperty("tokenType")
                    .GetString());
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "integration-admin@test.local",
                Password = "WrongPassword123"
            };

            // Act
            var response =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    loginRequest);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);

            Assert.DoesNotContain(
                "token",
                responseBody.ToLowerInvariant());
        }

        [Fact]
        public async Task Register_WithValidCustomerDetails_ReturnsOk()
        {
            // Arrange
            var registerRequest = new
            {
                FullName = "Integration Test Customer",
                Email = "integration-customer@example.com",
                PhoneNumber = "9876543210",
                Password = "Customer123",
                ConfirmPassword = "Customer123"
            };

            // Act
            var response =
                await _client.PostAsJsonAsync(
                    "/api/Auth/register",
                    registerRequest);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var responseJson =
                jsonDocument.RootElement;

            Assert.Equal(
                "User registered successfully.",
                responseJson
                    .GetProperty("message")
                    .GetString());

            Assert.Equal(
                "Customer",
                responseJson
                    .GetProperty("role")
                    .GetString());
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerRequest = new
            {
                FullName = "Duplicate Test Customer",
                Email = "duplicate-customer@example.com",
                PhoneNumber = "9876543211",
                Password = "Customer123",
                ConfirmPassword = "Customer123"
            };

            // Registers the customer for the first time.
            var firstResponse =
                await _client.PostAsJsonAsync(
                    "/api/Auth/register",
                    registerRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                firstResponse.StatusCode);

            // Act: tries to register the same email again.
            var secondResponse =
                await _client.PostAsJsonAsync(
                    "/api/Auth/register",
                    registerRequest);

            var responseBody =
                await secondResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                secondResponse.StatusCode);

            Assert.Contains(
                "A user with this email already exists.",
                responseBody);
        }
    }
}