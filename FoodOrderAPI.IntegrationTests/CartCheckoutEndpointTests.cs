using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace FoodOrderAPI.IntegrationTests
{
    // Contains focused integration tests for cart checkout.
    public class CartCheckoutEndpointTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        // Creates an HTTP client connected to the test API.
        public CartCheckoutEndpointTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ---------------------------------------------------------
        // UNAUTHENTICATED CHECKOUT
        // ---------------------------------------------------------

        [Fact]
        public async Task Checkout_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Cart/checkout");

            // Act
            var response =
                await _client.SendAsync(request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        // ---------------------------------------------------------
        // ADMIN CHECKOUT
        // ---------------------------------------------------------

        [Fact]
        public async Task Checkout_WithAdminToken_ReturnsForbidden()
        {
            // Arrange
            var adminToken =
                await CreateAdminTokenAsync();

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Cart/checkout");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            // Act
            var response =
                await _client.SendAsync(request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        // ---------------------------------------------------------
        // EMPTY CART
        // ---------------------------------------------------------

        [Fact]
        public async Task Checkout_WithEmptyCart_ReturnsBadRequest()
        {
            // Arrange
            var customerToken =
                await CreateCustomerTokenAsync();

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Cart/checkout");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            // Act
            var response =
                await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var result =
                jsonDocument.RootElement;

            Assert.False(
                result
                    .GetProperty("isSuccess")
                    .GetBoolean());

            Assert.Equal(
                "The cart is empty. Add at least one item " +
                "before checkout.",
                result
                    .GetProperty("message")
                    .GetString());

            Assert.Equal(
                JsonValueKind.Null,
                result
                    .GetProperty("order")
                    .ValueKind);
        }

        // ---------------------------------------------------------
        // SUCCESSFUL CHECKOUT
        // ---------------------------------------------------------

        [Fact]
        public async Task Checkout_WithValidCart_CreatesOrderAndClearsItems()
        {
            // Arrange: create an isolated customer.
            var customerToken =
                await CreateCustomerTokenAsync();

            // Adds the seeded food item to the customer's cart.
            var addCartItemRequest = new
            {
                FoodItemId = 10001,
                Quantity = 2
            };

            using var addRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Cart/items")
                {
                    Content =
                        JsonContent.Create(
                            addCartItemRequest)
                };

            addRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var addResponse =
                await _client.SendAsync(addRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                addResponse.StatusCode);

            var addResponseBody =
                await addResponse.Content
                    .ReadAsStringAsync();

            using var addJsonDocument =
                JsonDocument.Parse(addResponseBody);

            // Stores the original cart ID so that the test
            // can verify the same cart remains after checkout.
            var originalCartId =
                addJsonDocument.RootElement
                    .GetProperty("id")
                    .GetInt32();

            using var checkoutRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Cart/checkout");

            checkoutRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            // Act: checkout the customer's cart.
            var checkoutResponse =
                await _client.SendAsync(
                    checkoutRequest);

            var checkoutResponseBody =
                await checkoutResponse.Content
                    .ReadAsStringAsync();

            // Assert: checkout created the expected order.
            Assert.Equal(
                HttpStatusCode.OK,
                checkoutResponse.StatusCode);

            using var checkoutJsonDocument =
                JsonDocument.Parse(
                    checkoutResponseBody);

            var checkoutResult =
                checkoutJsonDocument.RootElement;

            Assert.True(
                checkoutResult
                    .GetProperty("isSuccess")
                    .GetBoolean());

            Assert.Equal(
                "Order created successfully.",
                checkoutResult
                    .GetProperty("message")
                    .GetString());

            var createdOrder =
                checkoutResult.GetProperty("order");

            var createdOrderId =
                createdOrder
                    .GetProperty("id")
                    .GetInt32();

            Assert.True(createdOrderId > 0);

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

            var createdOrderItems =
                createdOrder.GetProperty("items");

            Assert.Equal(
                1,
                createdOrderItems.GetArrayLength());

            var createdOrderItem =
                createdOrderItems[0];

            Assert.Equal(
                10001,
                createdOrderItem
                    .GetProperty("foodItemId")
                    .GetInt32());

            Assert.Equal(
                "Paneer Fried Rice",
                createdOrderItem
                    .GetProperty("foodItemName")
                    .GetString());

            Assert.Equal(
                2,
                createdOrderItem
                    .GetProperty("quantity")
                    .GetInt32());

            Assert.Equal(
                180m,
                createdOrderItem
                    .GetProperty("unitPrice")
                    .GetDecimal());

            // Retrieves the cart after checkout.
            using var getCartRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/Cart");

            getCartRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var getCartResponse =
                await _client.SendAsync(
                    getCartRequest);

            var getCartResponseBody =
                await getCartResponse.Content
                    .ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                getCartResponse.StatusCode);

            using var cartJsonDocument =
                JsonDocument.Parse(
                    getCartResponseBody);

            var cart =
                cartJsonDocument.RootElement;

            // The original cart remains in the database.
            Assert.Equal(
                originalCartId,
                cart.GetProperty("id").GetInt32());

            // Only its CartItems were deleted.
            Assert.Equal(
                0,
                cart
                    .GetProperty("items")
                    .GetArrayLength());

            Assert.Equal(
                0m,
                cart
                    .GetProperty("totalAmount")
                    .GetDecimal());

            // Verifies that the created order remains available.
            using var getOrdersRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/Order/my-orders");

            getOrdersRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var getOrdersResponse =
                await _client.SendAsync(
                    getOrdersRequest);

            var getOrdersResponseBody =
                await getOrdersResponse.Content
                    .ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                getOrdersResponse.StatusCode);

            using var ordersJsonDocument =
                JsonDocument.Parse(
                    getOrdersResponseBody);

            var createdOrderWasReturned = false;

            foreach (var order in
                ordersJsonDocument.RootElement
                    .EnumerateArray())
            {
                if (order
                    .GetProperty("id")
                    .GetInt32() == createdOrderId)
                {
                    createdOrderWasReturned = true;
                    break;
                }
            }

            Assert.True(createdOrderWasReturned);
        }

        // ---------------------------------------------------------
        // UNAVAILABLE FOOD ITEM
        // ---------------------------------------------------------

        [Fact]
        public async Task Checkout_WhenItemBecomesUnavailable_PreservesCart()
        {
            // Arrange: creates a unique food item so this test
            // does not modify shared seeded food-item data.
            var adminToken =
                await CreateAdminTokenAsync();

            var uniqueName =
                $"Checkout Item {Guid.NewGuid():N}";

            var createFoodItemRequest = new
            {
                Name = uniqueName,
                Description =
                    "Food item created for checkout testing",
                Price = 75m,
                FoodCategoryId = 10001,
                IsAvailable = true
            };

            using var createRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/FoodItems")
                {
                    Content =
                        JsonContent.Create(
                            createFoodItemRequest)
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
                await createResponse.Content
                    .ReadAsStringAsync();

            using var createJsonDocument =
                JsonDocument.Parse(
                    createResponseBody);

            var foodItemId =
                createJsonDocument.RootElement
                    .GetProperty("id")
                    .GetInt32();

            // Creates a separate customer for this test.
            var customerToken =
                await CreateCustomerTokenAsync();

            // Adds the available item to the customer's cart.
            var addCartItemRequest = new
            {
                FoodItemId = foodItemId,
                Quantity = 1
            };

            using var addRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Cart/items")
                {
                    Content =
                        JsonContent.Create(
                            addCartItemRequest)
                };

            addRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var addResponse =
                await _client.SendAsync(addRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                addResponse.StatusCode);

            // Makes the food item unavailable after it
            // has already been added to the cart.
            var updateFoodItemRequest = new
            {
                Name = uniqueName,
                Description =
                    "Food item created for checkout testing",
                Price = 75m,
                FoodCategoryId = 10001,
                IsAvailable = false
            };

            using var updateRequest =
                new HttpRequestMessage(
                    HttpMethod.Put,
                    $"/api/FoodItems/{foodItemId}")
                {
                    Content =
                        JsonContent.Create(
                            updateFoodItemRequest)
                };

            updateRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    adminToken);

            var updateResponse =
                await _client.SendAsync(updateRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                updateResponse.StatusCode);

            using var checkoutRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Cart/checkout");

            checkoutRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            // Act
            var checkoutResponse =
                await _client.SendAsync(
                    checkoutRequest);

            var checkoutResponseBody =
                await checkoutResponse.Content
                    .ReadAsStringAsync();

            // Assert: checkout is rejected.
            Assert.Equal(
                HttpStatusCode.BadRequest,
                checkoutResponse.StatusCode);

            using var checkoutJsonDocument =
                JsonDocument.Parse(
                    checkoutResponseBody);

            var checkoutResult =
                checkoutJsonDocument.RootElement;

            Assert.False(
                checkoutResult
                    .GetProperty("isSuccess")
                    .GetBoolean());

            Assert.Equal(
                $"{uniqueName} is currently unavailable.",
                checkoutResult
                    .GetProperty("message")
                    .GetString());

            // The rejected checkout must preserve the cart item.
            using var getCartRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/Cart");

            getCartRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var getCartResponse =
                await _client.SendAsync(
                    getCartRequest);

            var getCartResponseBody =
                await getCartResponse.Content
                    .ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                getCartResponse.StatusCode);

            using var cartJsonDocument =
                JsonDocument.Parse(
                    getCartResponseBody);

            var cartItems =
                cartJsonDocument.RootElement
                    .GetProperty("items");

            Assert.Equal(
                1,
                cartItems.GetArrayLength());

            Assert.Equal(
                foodItemId,
                cartItems[0]
                    .GetProperty("foodItemId")
                    .GetInt32());

            Assert.False(
                cartItems[0]
                    .GetProperty("isAvailable")
                    .GetBoolean());

            // No order should have been created.
            using var getOrdersRequest =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/Order/my-orders");

            getOrdersRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    customerToken);

            var getOrdersResponse =
                await _client.SendAsync(
                    getOrdersRequest);

            var getOrdersResponseBody =
                await getOrdersResponse.Content
                    .ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                getOrdersResponse.StatusCode);

            using var ordersJsonDocument =
                JsonDocument.Parse(
                    getOrdersResponseBody);

            Assert.Equal(
                0,
                ordersJsonDocument.RootElement
                    .GetArrayLength());
        }

        // ---------------------------------------------------------
        // CUSTOMER TOKEN HELPER
        // ---------------------------------------------------------

        // Registers a unique customer and returns their JWT.
        private async Task<string> CreateCustomerTokenAsync()
        {
            var uniqueValue =
                Guid.NewGuid().ToString("N");

            var customerEmail =
                $"cart-checkout-{uniqueValue}@example.com";

            const string customerPassword =
                "Customer123";

            var registerRequest = new
            {
                FullName = "Cart Checkout Customer",
                Email = customerEmail,
                PhoneNumber = "9876543214",
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

        // ---------------------------------------------------------
        // ADMIN TOKEN HELPER
        // ---------------------------------------------------------

        // Logs in as the seeded integration-test Admin.
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