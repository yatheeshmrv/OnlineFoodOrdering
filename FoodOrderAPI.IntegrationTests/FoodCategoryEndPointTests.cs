using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using Xunit;    

namespace FoodOrderAPI.IntegrationTests
{
    public class FoodCategoryEndpointTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public FoodCategoryEndpointTests(
            CustomWebApplicationFactory factory)
        {
            // Creates an HTTP client connected to the test API.
            _client = factory.CreateClient();
        }

        // Test for retrieving all food categories
        [Fact]
        public async Task GetFoodCategories_WhenCategoryExists_ReturnsOkWithCategory()
        {
            // Act
            var response =
                await _client.GetAsync(
                    "/api/FoodCategory");

            var responseBody =
                await response.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            Assert.Contains(
                "Healthy Meals",
                responseBody);
        }

        // Test for a specific category ID that exists
        [Fact]
        public async Task GetFoodCategoryById_WhenCategoryExists_ReturnsOk()
        {
            // Act
            var response =
                await _client.GetAsync(
                    "/api/FoodCategory/10001");

            var responseBody =
                await response.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            Assert.Contains(
                "Healthy Meals",
                responseBody);
        }

        // Test for a non-existing category ID
        [Fact]
        public async Task GetFoodCategoryById_WhenCategoryDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response =
                await _client.GetAsync(
                    "/api/FoodCategory/99999");

            // Assert
            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        // Test for creating a food category with an admin token
        [Fact]
        public async Task CreateFoodCategory_WithAdminToken_ReturnsCreated()
        {
            // Arrange
            var adminToken =
                await CreateAdminTokenAsync();

            var categoryName =
            $"Desserts-{Guid.NewGuid():N}";

            var categoryRequest = new
            {
                CategoryName = categoryName,
                IsActive = true
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/FoodCategory")
                {
                    Content =
                        JsonContent.Create(categoryRequest)
                };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var response =
                await _client.SendAsync(request);

            var responseBody =
                await response.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var responseJson =
                jsonDocument.RootElement;

            Assert.Equal(
            categoryName,
            responseJson
            .GetProperty("categoryName")
            .GetString());

            Assert.True(
                responseJson
                    .GetProperty("isActive")
                    .GetBoolean());
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

        // Test for updating a food category with an admin token
        [Fact]
        public async Task UpdateFoodCategory_WithAdminToken_ReturnsOk()
        {
            // Arrange
            var adminToken =
                await CreateAdminTokenAsync();

            var createRequest = new
            {
                CategoryName = "Beverages",
                IsActive = true
            };

            using var createMessage =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/FoodCategory")
                {
                    Content = JsonContent.Create(createRequest)
                };

            createMessage.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var createResponse =
                await _client.SendAsync(createMessage);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createBody =
                await createResponse.Content
                    .ReadAsStringAsync();

            using var createJson =
                JsonDocument.Parse(createBody);

            var categoryId =
                createJson.RootElement
                    .GetProperty("id")
                    .GetInt32();

            var updateRequest = new
            {
                CategoryName = "Cold Beverages",
                IsActive = false
            };

            using var updateMessage =
                new HttpRequestMessage(
                    HttpMethod.Put,
                    $"/api/FoodCategory/{categoryId}")
                {
                    Content = JsonContent.Create(updateRequest)
                };

            updateMessage.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var updateResponse =
                await _client.SendAsync(updateMessage);

            var updateBody =
                await updateResponse.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                updateResponse.StatusCode);

            using var updateJson =
                JsonDocument.Parse(updateBody);

            Assert.Equal(
                "Cold Beverages",
                updateJson.RootElement
                    .GetProperty("categoryName")
                    .GetString());

            Assert.False(
                updateJson.RootElement
                    .GetProperty("isActive")
                    .GetBoolean());
        }

        // Test for deleting a food category with an admin token
        [Fact]
        public async Task DeleteFoodCategory_WithAdminToken_ReturnsNoContent()
        {
            // Arrange
            var adminToken =
                await CreateAdminTokenAsync();

            var categoryName =
                $"Delete-{Guid.NewGuid():N}";

            var createRequest = new
            {
                CategoryName = categoryName,
                IsActive = true
            };

            using var createMessage =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/FoodCategory")
                {
                    Content = JsonContent.Create(createRequest)
                };

            createMessage.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var createResponse =
                await _client.SendAsync(createMessage);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createBody =
                await createResponse.Content
                    .ReadAsStringAsync();

            using var createJson =
                JsonDocument.Parse(createBody);

            var categoryId =
                createJson.RootElement
                    .GetProperty("id")
                    .GetInt32();

            using var deleteMessage =
                new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"/api/FoodCategory/{categoryId}");

            deleteMessage.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var deleteResponse =
                await _client.SendAsync(deleteMessage);

            // Assert
            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteResponse.StatusCode);

            // Confirms that the category no longer exists.
            var getResponse =
                await _client.GetAsync(
                    $"/api/FoodCategory/{categoryId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                getResponse.StatusCode);
        }

        // Test for creating a food category with an empty name, expecting a BadRequest response
        [Fact]
        public async Task CreateFoodCategory_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var adminToken =
                await CreateAdminTokenAsync();

            var categoryRequest = new
            {
                CategoryName = "",
                IsActive = true
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/FoodCategory")
                {
                    Content =
                        JsonContent.Create(categoryRequest)
                };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var response =
                await _client.SendAsync(request);

            var responseBody =
                await response.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.Contains(
                "Validation failed.",
                responseBody);

            Assert.Contains(
                "CategoryName",
                responseBody);
        }
    }
}