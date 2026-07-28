using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
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

        // Test to ensure that adding a food item without authentication returns Unauthorized.
        [Fact]
        public async Task AddFoodItem_WithoutAuthentication_ReturnsUnauthorized()
        {
            var request = new
            {
                Name = $"Unauthorized Item {Guid.NewGuid():N}",
                Description = "Created without an authentication token",
                Price = 150m,
                FoodCategoryId = 10001,
                IsAvailable = true
            };

            var response = await _client.PostAsJsonAsync(
                "/api/FoodItems",
                request);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        // Helper method to create a customer and obtain a JWT token for testing.
        private async Task<string> CreateCustomerTokenAsync()
        {
            var uniqueValue = Guid.NewGuid().ToString("N");
            var email = $"fooditem-customer-{uniqueValue}@test.local";
            const string password = "Customer@123";

            var registration = new
            {
                FullName = "Food Item Test Customer",
                Email = email,
                PhoneNumber = "9876543210",
                Password = password,
                ConfirmPassword = password
            };

            var registrationResponse = await _client.PostAsJsonAsync(
                "/api/Auth/register",
                registration);

            registrationResponse.EnsureSuccessStatusCode();

            var login = new
            {
                Email = email,
                Password = password
            };

            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Auth/login",
                login);

            loginResponse.EnsureSuccessStatusCode();

            var responseBody = await loginResponse.Content.ReadAsStringAsync();

            using var jsonDocument = JsonDocument.Parse(responseBody);

            return jsonDocument.RootElement
                .GetProperty("token")
                .GetString()!;
        }

        // Test to ensure that adding a food item as a customer returns Forbidden.
        [Fact]
        public async Task AddFoodItem_AsCustomer_ReturnsForbidden()
        {
            var customerToken = await CreateCustomerTokenAsync();

            var foodItem = new
            {
                Name = $"Customer Item {Guid.NewGuid():N}",
                Description = "Attempted creation by a customer",
                Price = 175m,
                FoodCategoryId = 10001,
                IsAvailable = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/FoodItems")
            {
                Content = JsonContent.Create(foodItem)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var response = await _client.SendAsync(request);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        // Test to ensure that retrieving a food item by ID returns the correct item when it exists.
        [Fact]
        public async Task GetFoodItemById_WhenItemExists_ReturnsOkWithItem()
        {
            var response = await _client.GetAsync(
                "/api/FoodItems/10001");

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var foodItem = jsonDocument.RootElement;

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            Assert.Equal(
                10001,
                foodItem.GetProperty("id").GetInt32());

            Assert.Equal(
                "Paneer Fried Rice",
                foodItem.GetProperty("name").GetString());

            Assert.Equal(
                180m,
                foodItem.GetProperty("price").GetDecimal());

            Assert.Equal(
                "Healthy Meals",
                foodItem.GetProperty("foodCategoryName").GetString());
        }

        // Test to ensure that retrieving a food item by ID returns NotFound when the item does not exist.
        [Fact]
        public async Task GetFoodItemById_WhenItemDoesNotExist_ReturnsNotFound()
        {
            var response = await _client.GetAsync(
                "/api/FoodItems/999999999");

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);

            Assert.Equal(
                "Food item not found.",
                jsonDocument.RootElement
                    .GetProperty("message")
                    .GetString());
        }

        // Test to ensure that retrieving food items with invalid pagination parameters returns BadRequest with validation errors.
        [Fact]
        public async Task GetFoodItems_WithInvalidPagination_ReturnsBadRequest()
        {
            var response = await _client.GetAsync(
                "/api/FoodItems?pageNumber=0&pageSize=0");

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var responseJson = jsonDocument.RootElement;

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.Equal(
                "Validation failed.",
                responseJson
                    .GetProperty("message")
                    .GetString());

            Assert.Equal(
                JsonValueKind.Object,
                responseJson
                    .GetProperty("errors")
                    .ValueKind);
        }

        // Test to ensure that retrieving food items with valid search and category parameters returns matching items with correct pagination.
        [Fact]
        public async Task GetFoodItems_WithValidSearchCategoryAndPagination_ReturnsMatchingItems()
        {
            var response = await _client.GetAsync(
                "/api/FoodItems?search=paneer&categoryId=10001&pageNumber=1&pageSize=10");

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var result = jsonDocument.RootElement;
            var items = result.GetProperty("items");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            Assert.Equal(
                1,
                result.GetProperty("pageNumber").GetInt32());

            Assert.Equal(
                10,
                result.GetProperty("pageSize").GetInt32());

            Assert.Equal(
                JsonValueKind.Array,
                items.ValueKind);

            Assert.True(items.GetArrayLength() > 0);

            Assert.Equal(
                "Paneer Fried Rice",
                items[0].GetProperty("name").GetString());

            Assert.Equal(
                10001,
                items[0]
                    .GetProperty("foodCategoryId")
                    .GetInt32());
        }

        // Helper method to create an admin token for testing.
        private async Task<string> CreateAdminTokenAsync()
        {
            var loginRequest = new
            {
                Email = "integration-admin@test.local",
                Password = "IntegrationTest@12345"
            };

            var loginResponse = await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var responseBody =
                await loginResponse.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            return jsonDocument.RootElement
                .GetProperty("token")
                .GetString()!;
        }

        // Test to ensure that adding a food item as an admin with valid data returns the created item with correct properties.
        [Fact]
        public async Task AddFoodItem_AsAdminWithValidData_ReturnsCreatedItem()
        {
            var adminToken = await CreateAdminTokenAsync();
            var uniqueName = $"Admin Food Item {Guid.NewGuid():N}";

            var foodItem = new
            {
                Name = $"  {uniqueName}  ",
                Description = "  Food item created by integration testing  ",
                Price = 245.50m,
                FoodCategoryId = 10001,
                IsAvailable = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/FoodItems")
            {
                Content = JsonContent.Create(foodItem)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var response = await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var createdItem = jsonDocument.RootElement;

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            Assert.True(
                createdItem.GetProperty("id").GetInt32() > 0);

            Assert.Equal(
                uniqueName,
                createdItem.GetProperty("name").GetString());

            Assert.Equal(
                "Food item created by integration testing",
                createdItem.GetProperty("description").GetString());

            Assert.Equal(
                245.50m,
                createdItem.GetProperty("price").GetDecimal());

            Assert.Equal(
                10001,
                createdItem
                    .GetProperty("foodCategoryId")
                    .GetInt32());

            Assert.True(
                createdItem
                    .GetProperty("isAvailable")
                    .GetBoolean());
        }

        // Test to ensure that adding a food item as an admin with invalid data returns BadRequest with validation errors.
        [Fact]
        public async Task AddFoodItem_AsAdminWithInvalidData_ReturnsBadRequest()
        {
            var adminToken = await CreateAdminTokenAsync();

            var foodItem = new
            {
                Name = string.Empty,
                Description = string.Empty,
                Price = 0m,
                FoodCategoryId = 0,
                IsAvailable = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/FoodItems")
            {
                Content = JsonContent.Create(foodItem)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var response = await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var responseJson = jsonDocument.RootElement;
            var errors = responseJson.GetProperty("errors");
            var errorNames = errors
                .EnumerateObject()
                .Select(error => error.Name)
                .ToList();

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.Equal(
                "Validation failed.",
                responseJson
                    .GetProperty("message")
                    .GetString());

            Assert.Contains(
                errorNames,
                name => string.Equals(
                    name,
                    "Name",
                    StringComparison.OrdinalIgnoreCase));

            Assert.Contains(
                errorNames,
                name => string.Equals(
                    name,
                    "Description",
                    StringComparison.OrdinalIgnoreCase));

            Assert.Contains(
                errorNames,
                name => string.Equals(
                    name,
                    "Price",
                    StringComparison.OrdinalIgnoreCase));

            Assert.Contains(
                errorNames,
                name => string.Equals(
                    name,
                    "FoodCategoryId",
                    StringComparison.OrdinalIgnoreCase));
        }

        // Test to ensure that adding a food item as an admin with an unknown category returns BadRequest with an appropriate message.
        [Fact]
        public async Task AddFoodItem_AsAdminWithUnknownCategory_ReturnsBadRequest()
        {
            var adminToken = await CreateAdminTokenAsync();

            var foodItem = new
            {
                Name = $"Unknown Category Item {Guid.NewGuid():N}",
                Description = "Uses a category that does not exist",
                Price = 210m,
                FoodCategoryId = 999999999,
                IsAvailable = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/FoodItems")
            {
                Content = JsonContent.Create(foodItem)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var response = await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var message = jsonDocument.RootElement
                .GetProperty("message")
                .GetString();

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.False(
                string.IsNullOrWhiteSpace(message));

            Assert.True(
                message!.Contains(
                    "category",
                    StringComparison.OrdinalIgnoreCase));
        }

        // Test to ensure that updating a food item as an admin with valid data returns the updated item with correct properties.
        [Fact]
        public async Task UpdateFoodItem_AsAdminWithValidData_ReturnsUpdatedItem()
        {
            var adminToken = await CreateAdminTokenAsync();
            var initialName = $"Update Test Item {Guid.NewGuid():N}";

            var createData = new
            {
                Name = initialName,
                Description = "Original description",
                Price = 190m,
                FoodCategoryId = 10001,
                IsAvailable = true
            };

            using var createRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/FoodItems")
            {
                Content = JsonContent.Create(createData)
            };

            createRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var createResponse =
                await _client.SendAsync(createRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createResponseBody =
                await createResponse.Content.ReadAsStringAsync();

            using var createJsonDocument =
                JsonDocument.Parse(createResponseBody);

            var foodItemId = createJsonDocument.RootElement
                .GetProperty("id")
                .GetInt32();

            var updatedName =
                $"Updated Food Item {Guid.NewGuid():N}";

            var updateData = new
            {
                Name = $"  {updatedName}  ",
                Description = "  Updated description  ",
                Price = 275m,
                FoodCategoryId = 10001,
                IsAvailable = false
            };

            using var updateRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"/api/FoodItems/{foodItemId}")
            {
                Content = JsonContent.Create(updateData)
            };

            updateRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var updateResponse =
                await _client.SendAsync(updateRequest);

            var updateResponseBody =
                await updateResponse.Content.ReadAsStringAsync();

            using var updateJsonDocument =
                JsonDocument.Parse(updateResponseBody);

            var updatedItem =
                updateJsonDocument.RootElement;

            Assert.Equal(
                HttpStatusCode.OK,
                updateResponse.StatusCode);

            Assert.Equal(
                foodItemId,
                updatedItem.GetProperty("id").GetInt32());

            Assert.Equal(
                updatedName,
                updatedItem.GetProperty("name").GetString());

            Assert.Equal(
                "Updated description",
                updatedItem
                    .GetProperty("description")
                    .GetString());

            Assert.Equal(
                275m,
                updatedItem.GetProperty("price").GetDecimal());

            Assert.False(
                updatedItem
                    .GetProperty("isAvailable")
                    .GetBoolean());
        }

        // Test to ensure that updating a food item as an admin with invalid data returns BadRequest with validation errors.
        [Fact]
        public async Task UpdateFoodItem_AsAdminWithInvalidData_ReturnsBadRequest()
        {
            var adminToken = await CreateAdminTokenAsync();

            var updateData = new
            {
                Name = string.Empty,
                Description = string.Empty,
                Price = 0m,
                FoodCategoryId = 0,
                IsAvailable = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                "/api/FoodItems/10001")
            {
                Content = JsonContent.Create(updateData)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var response = await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var responseJson = jsonDocument.RootElement;
            var errors = responseJson.GetProperty("errors");
            var errorNames = errors
                .EnumerateObject()
                .Select(error => error.Name)
                .ToList();

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.Equal(
                "Validation failed.",
                responseJson
                    .GetProperty("message")
                    .GetString());

            Assert.Contains(
                errorNames,
                name => string.Equals(
                    name,
                    "Name",
                    StringComparison.OrdinalIgnoreCase));

            Assert.Contains(
                errorNames,
                name => string.Equals(
                    name,
                    "Description",
                    StringComparison.OrdinalIgnoreCase));

            Assert.Contains(
                errorNames,
                name => string.Equals(
                    name,
                    "Price",
                    StringComparison.OrdinalIgnoreCase));

            Assert.Contains(
                errorNames,
                name => string.Equals(
                    name,
                    "FoodCategoryId",
                    StringComparison.OrdinalIgnoreCase));
        }

        // Test to ensure that updating a food item as an admin when the item does not exist returns NotFound with an appropriate message.
        [Fact]
        public async Task UpdateFoodItem_AsAdminWhenItemDoesNotExist_ReturnsNotFound()
        {
            var adminToken = await CreateAdminTokenAsync();

            var updateData = new
            {
                Name = $"Missing Item {Guid.NewGuid():N}",
                Description = "Valid update for a nonexistent item",
                Price = 225m,
                FoodCategoryId = 10001,
                IsAvailable = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                "/api/FoodItems/999999999")
            {
                Content = JsonContent.Create(updateData)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var response = await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);

            Assert.Equal(
                "Food item not found.",
                jsonDocument.RootElement
                    .GetProperty("message")
                    .GetString());
        }

        // Test to ensure that updating a food item as an admin with an unknown category returns BadRequest with an appropriate message.    
        [Fact]
        public async Task UpdateFoodItem_AsAdminWithUnknownCategory_ReturnsBadRequest()
        {
            var adminToken = await CreateAdminTokenAsync();

            var updateData = new
            {
                Name = $"Unknown Category Update {Guid.NewGuid():N}",
                Description = "Update using a nonexistent category",
                Price = 260m,
                FoodCategoryId = 999999999,
                IsAvailable = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                "/api/FoodItems/10001")
            {
                Content = JsonContent.Create(updateData)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var response = await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var message = jsonDocument.RootElement
                .GetProperty("message")
                .GetString();

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.False(
                string.IsNullOrWhiteSpace(message));

            Assert.True(
                message!.Contains(
                    "category",
                    StringComparison.OrdinalIgnoreCase));
        }

        // Test to ensure that deleting a food item as an admin when the item exists returns NoContent and the item is no longer retrievable.
        [Fact]
        public async Task DeleteFoodItem_AsAdminWhenItemExists_ReturnsNoContent()
        {
            var adminToken = await CreateAdminTokenAsync();

            var createData = new
            {
                Name = $"Delete Test Item {Guid.NewGuid():N}",
                Description = "Food item created for deletion testing",
                Price = 230m,
                FoodCategoryId = 10001,
                IsAvailable = true
            };

            using var createRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/FoodItems")
            {
                Content = JsonContent.Create(createData)
            };

            createRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var createResponse =
                await _client.SendAsync(createRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createResponseBody =
                await createResponse.Content.ReadAsStringAsync();

            using var createJsonDocument =
                JsonDocument.Parse(createResponseBody);

            var foodItemId = createJsonDocument.RootElement
                .GetProperty("id")
                .GetInt32();

            using var deleteRequest = new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/FoodItems/{foodItemId}");

            deleteRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var deleteResponse =
                await _client.SendAsync(deleteRequest);

            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync(
                $"/api/FoodItems/{foodItemId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                getResponse.StatusCode);
        }

        // Test to ensure that deleting a food item as an admin when the item does not exist returns NotFound with an appropriate message.
        [Fact]
        public async Task DeleteFoodItem_AsAdminWhenItemDoesNotExist_ReturnsNotFound()
        {
            var adminToken = await CreateAdminTokenAsync();

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                "/api/FoodItems/2147483647");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var response = await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);

            Assert.Equal(
                "Food item not found.",
                jsonDocument.RootElement
                    .GetProperty("message")
                    .GetString());
        }
    }
}