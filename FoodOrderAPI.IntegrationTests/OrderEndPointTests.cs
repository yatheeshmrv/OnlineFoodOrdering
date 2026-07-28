using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace FoodOrderAPI.IntegrationTests
{
    // This class contains integration tests for the Order API endpoints.
    public class OrderEndpointTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        // Constructor to initialize the HTTP client for integration tests
        public OrderEndpointTests(
            CustomWebApplicationFactory factory)
        {
            // Creates an HTTP client connected to the test API.
            _client = factory.CreateClient();
        }

        // Test to verify that a customer can create an order with a valid token
        [Fact]
        public async Task CreateOrder_WithValidCustomerToken_ReturnsCreated()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderRequest = new
            {
                Items = new[]
                {
                    new
                    {
                        FoodItemId = 10001,
                        Quantity = 2
                    }
                }
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

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

            Assert.True(
                responseJson
                    .GetProperty("isSuccess")
                    .GetBoolean());

            var createdOrder =
                responseJson.GetProperty("order");

            Assert.Equal(
                360m,
                createdOrder
                    .GetProperty("totalAmount")
                    .GetDecimal());

            Assert.Equal(
                "Pending",
                createdOrder
                    .GetProperty("orderStatus")
                    .GetString());
        }

        // Helper method to create a new customer and retrieve their JWT token
        private async Task<string> CreateCustomerTokenAsync()
        {
            var uniqueValue =
                Guid.NewGuid().ToString("N");

            var customerEmail =
                $"order-customer-{uniqueValue}@example.com";

            const string customerPassword =
                "Customer123";

            var registerRequest = new
            {
                FullName = "Order Test Customer",
                Email = customerEmail,
                PhoneNumber = "9876543213",
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

        // Test to verify that a customer can retrieve their own orders
        [Fact]
        public async Task GetMyOrders_WithCustomerToken_ReturnsCustomerOrder()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderRequest = new
            {
                Items = new[]
                {
            new
            {
                FoodItemId = 10001,
                Quantity = 1
            }
        }
            };

            using var createRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            createRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var createResponse =
                await _client.SendAsync(createRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createBody =
                await createResponse.Content
                    .ReadAsStringAsync();

            using var createJson =
                JsonDocument.Parse(createBody);

            var createdOrderId =
                createJson.RootElement
                    .GetProperty("order")
                    .GetProperty("id")
                    .GetInt32();

            using var getRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/Order/my-orders");

            getRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            // Act
            var getResponse =
                await _client.SendAsync(getRequest);

            var getBody =
                await getResponse.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            using var getJson =
                JsonDocument.Parse(getBody);

            var orderWasReturned = false;

            foreach (var order in
                getJson.RootElement.EnumerateArray())
            {
                if (order.GetProperty("id").GetInt32()
                    == createdOrderId)
                {
                    orderWasReturned = true;
                    break;
                }
            }

            Assert.True(orderWasReturned);
        }

        // Test to verify that a customer can retrieve a specific order by ID if it belongs to them
        [Fact]
        public async Task GetMyOrderById_WhenOrderBelongsToCustomer_ReturnsOk()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderRequest = new
            {
                Items = new[]
                {
            new
            {
                FoodItemId = 10001,
                Quantity = 3
            }
        }
            };

            using var createRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            createRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var createResponse =
                await _client.SendAsync(createRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createBody =
                await createResponse.Content
                    .ReadAsStringAsync();

            using var createJson =
                JsonDocument.Parse(createBody);

            var createdOrderId =
                createJson.RootElement
                    .GetProperty("order")
                    .GetProperty("id")
                    .GetInt32();

            using var getRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"/api/Order/my-orders/{createdOrderId}");

            getRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            // Act
            var getResponse =
                await _client.SendAsync(getRequest);

            var getBody =
                await getResponse.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            using var getJson =
                JsonDocument.Parse(getBody);

            Assert.Equal(
                createdOrderId,
                getJson.RootElement
                    .GetProperty("id")
                    .GetInt32());

            Assert.Equal(
                540m,
                getJson.RootElement
                    .GetProperty("totalAmount")
                    .GetDecimal());
        }

        // Test to verify that a customer cannot retrieve an order that belongs to another customer
        [Fact]
        public async Task GetMyOrderById_WhenOrderBelongsToAnotherCustomer_ReturnsNotFound()
        {
            // Arrange
            var firstCustomerToken =
                await CreateCustomerTokenAsync();

            var secondCustomerToken =
                await CreateCustomerTokenAsync();

            var orderRequest = new
            {
                Items = new[]
                {
            new
            {
                FoodItemId = 10001,
                Quantity = 1
            }
        }
            };

            using var createRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            createRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    firstCustomerToken);

            var createResponse =
                await _client.SendAsync(createRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createBody =
                await createResponse.Content
                    .ReadAsStringAsync();

            using var createJson =
                JsonDocument.Parse(createBody);

            var createdOrderId =
                createJson.RootElement
                    .GetProperty("order")
                    .GetProperty("id")
                    .GetInt32();

            using var getRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"/api/Order/my-orders/{createdOrderId}");

            // Uses the second customer’s token.
            getRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    secondCustomerToken);

            // Act
            var getResponse =
                await _client.SendAsync(getRequest);

            // Assert
            Assert.Equal(
                HttpStatusCode.NotFound,
                getResponse.StatusCode);
        }

        // Test to verify that creating an order with empty items returns a Bad Request response
        [Fact]
        public async Task CreateOrder_WithEmptyItems_ReturnsBadRequest()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderRequest = new
            {
                Items = Array.Empty<object>()
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

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
                "Items",
                responseBody);
        }

        [Fact]
        public async Task CreateOrder_WithUnknownFoodItem_ReturnsBadRequest()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderRequest = new
            {
                Items = new[]
                {
            new
            {
                FoodItemId = 99999,
                Quantity = 1
            }
        }
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

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

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            Assert.False(
                jsonDocument.RootElement
                    .GetProperty("isSuccess")
                    .GetBoolean());

            Assert.Contains(
                "not found",
                jsonDocument.RootElement
                    .GetProperty("message")
                    .GetString()!,
                StringComparison.OrdinalIgnoreCase);
        }

        // Test to verify that an admin can retrieve all orders, including those created by customers
        [Fact]
        public async Task GetAllOrders_WithAdminToken_ReturnsCreatedOrder()
        {
            // Arrange: create an order as a customer.
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderRequest = new
            {
                Items = new[]
                {
            new
            {
                FoodItemId = 10001,
                Quantity = 1
            }
        }
            };

            using var createRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            createRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var createResponse =
                await _client.SendAsync(createRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createBody =
                await createResponse.Content
                    .ReadAsStringAsync();

            using var createJson =
                JsonDocument.Parse(createBody);

            var createdOrderId =
                createJson.RootElement
                    .GetProperty("order")
                    .GetProperty("id")
                    .GetInt32();

            var adminToken =
                await CreateAdminTokenAsync();

            using var getRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/Order");

            getRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var getResponse =
                await _client.SendAsync(getRequest);

            var getBody =
                await getResponse.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            using var getJson =
                JsonDocument.Parse(getBody);

            var orderWasReturned = false;

            foreach (var order in
                getJson.RootElement.EnumerateArray())
            {
                if (order.GetProperty("id").GetInt32()
                    == createdOrderId)
                {
                    orderWasReturned = true;
                    break;
                }
            }

            Assert.True(orderWasReturned);
        }

        // Test to verify that an admin can retrieve a specific order by ID
        [Fact]
        public async Task GetOrderById_WithAdminToken_ReturnsOrder()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderRequest = new
            {
                Items = new[]
                {
            new
            {
                FoodItemId = 10001,
                Quantity = 2
            }
        }
            };

            using var createRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            createRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var createResponse =
                await _client.SendAsync(createRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createBody =
                await createResponse.Content
                    .ReadAsStringAsync();

            using var createJson =
                JsonDocument.Parse(createBody);

            var createdOrderId =
                createJson.RootElement
                    .GetProperty("order")
                    .GetProperty("id")
                    .GetInt32();

            var adminToken =
                await CreateAdminTokenAsync();

            using var getRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"/api/Order/{createdOrderId}");

            getRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var getResponse =
                await _client.SendAsync(getRequest);

            var getBody =
                await getResponse.Content
                    .ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            using var getJson =
                JsonDocument.Parse(getBody);

            Assert.Equal(
                createdOrderId,
                getJson.RootElement
                    .GetProperty("id")
                    .GetInt32());

            Assert.Equal(
                360m,
                getJson.RootElement
                    .GetProperty("totalAmount")
                    .GetDecimal());
        }

        // Helper method to create an admin token for testing purposes
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

        // Helper method to create an order for testing purposes and return its ID
        private async Task<int> CreateOrderAsync(
             string customerToken,
             int quantity)
        {
            var orderRequest = new
            {
                Items = new[]
                {
            new
            {
                FoodItemId = 10001,
                Quantity = quantity
            }
        }
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Order")
                {
                    Content =
                        JsonContent.Create(orderRequest)
                };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var response =
                await _client.SendAsync(request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            var responseBody =
                await response.Content
                    .ReadAsStringAsync();

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            return jsonDocument.RootElement
                .GetProperty("order")
                .GetProperty("id")
                .GetInt32();
        }

        // Test to verify that an admin can update the status of an order
        [Fact]
        public async Task UpdateOrderStatus_WithAdminToken_ReturnsUpdatedOrder()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderId =
                await CreateOrderAsync(
                    customerToken,
                    1);

            var adminToken =
                await CreateAdminTokenAsync();

            var statusRequest = new
            {
                OrderStatus = "Confirmed"
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Put,
                    $"/api/Order/{orderId}/status")
                {
                    Content =
                        JsonContent.Create(statusRequest)
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
                HttpStatusCode.OK,
                response.StatusCode);

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            Assert.Equal(
                orderId,
                jsonDocument.RootElement
                    .GetProperty("id")
                    .GetInt32());

            Assert.Equal(
                "Confirmed",
                jsonDocument.RootElement
                    .GetProperty("orderStatus")
                    .GetString());
        }

        // Test to verify that updating an order status with an invalid status returns a Bad Request response
        [Fact]
        public async Task UpdateOrderStatus_WithInvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderId =
                await CreateOrderAsync(
                    customerToken,
                    1);

            var adminToken =
                await CreateAdminTokenAsync();

            var statusRequest = new
            {
                OrderStatus = "Invalid Status"
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Put,
                    $"/api/Order/{orderId}/status")
                {
                    Content =
                        JsonContent.Create(statusRequest)
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
                "OrderStatus",
                responseBody);
        }

        // Test to verify that an admin can delete an order and that the order is no longer retrievable afterward
        [Fact]
        public async Task DeleteOrder_WithAdminToken_ReturnsNoContent()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderId =
                await CreateOrderAsync(
                    customerToken,
                    1);

            var adminToken =
                await CreateAdminTokenAsync();

            using var deleteRequest =
                new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"/api/Order/{orderId}");

            deleteRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var deleteResponse =
                await _client.SendAsync(deleteRequest);

            // Assert
            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteResponse.StatusCode);

            // Confirms that the deleted order no longer exists.
            using var getRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"/api/Order/{orderId}");

            getRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var getResponse =
                await _client.SendAsync(getRequest);

            Assert.Equal(
                HttpStatusCode.NotFound,
                getResponse.StatusCode);
        }

        // Test to verify that attempting to delete a non-existent order returns a Not Found response
        [Fact]
        public async Task DeleteOrder_WhenOrderDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var adminToken =
                await CreateAdminTokenAsync();

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Delete,
                    "/api/Order/99999");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var response =
                await _client.SendAsync(request);

            // Assert
            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        // Test to verify that attempting to update the status of a non-existent order returns a Not Found response
        [Fact]
        public async Task UpdateOrderStatus_WhenOrderDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var adminToken =
                await CreateAdminTokenAsync();

            var statusRequest = new
            {
                OrderStatus = "Confirmed"
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Put,
                    "/api/Order/99999/status")
                {
                    Content =
                        JsonContent.Create(statusRequest)
                };

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var response =
                await _client.SendAsync(request);

            // Assert
            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }
    }
}