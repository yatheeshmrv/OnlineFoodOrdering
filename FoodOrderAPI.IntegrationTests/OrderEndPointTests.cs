using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace FoodOrderAPI.IntegrationTests
{
    // Integration tests for the production order flow:
    //
    // Save address → Add cart item → Checkout → Retrieve order.
    public class OrderEndpointTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public OrderEndpointTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ---------------------------------------------------------
        // SUCCESSFUL CHECKOUT
        // ---------------------------------------------------------

        [Fact]
        public async Task
            Checkout_WithValidCartAndAddress_ReturnsCreatedOrder()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var response =
                await CheckoutCartAsync(
                    customerToken,
                    quantity: 2);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
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

            Assert.Equal(
                "Order Test Customer",
                createdOrder
                    .GetProperty("deliveryRecipientName")
                    .GetString());

            Assert.Equal(
                "9876543213",
                createdOrder
                    .GetProperty("deliveryPhone")
                    .GetString());

            Assert.Equal(
                "12, MG Road",
                createdOrder
                    .GetProperty("deliveryAddressLine1")
                    .GetString());

            Assert.Equal(
                "Bengaluru",
                createdOrder
                    .GetProperty("deliveryCity")
                    .GetString());

            Assert.Equal(
                "Karnataka",
                createdOrder
                    .GetProperty("deliveryState")
                    .GetString());

            Assert.Equal(
                "560001",
                createdOrder
                    .GetProperty("deliveryPostalCode")
                    .GetString());

            Assert.Equal(
                "Call when you arrive.",
                createdOrder
                    .GetProperty("deliveryInstructions")
                    .GetString());
        }

        // ---------------------------------------------------------
        // EMPTY CART
        // ---------------------------------------------------------

        [Fact]
        public async Task
            Checkout_WhenCartIsEmpty_ReturnsBadRequest()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var addressId =
                await CreateAddressAsync(customerToken);

            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "/api/Cart/checkout",
                    customerToken,
                    new
                    {
                        UserAddressId = addressId,
                        DeliveryInstructions =
                            "Call when you arrive."
                    });

            var response =
                await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.Contains(
                "cart is empty",
                responseBody,
                StringComparison.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------
        // INVALID OR UNAUTHORIZED ADDRESS
        // ---------------------------------------------------------

        [Fact]
        public async Task
            Checkout_WithUnknownAddress_ReturnsBadRequest()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            await AddCartItemAsync(
                customerToken,
                quantity: 1);

            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "/api/Cart/checkout",
                    customerToken,
                    new
                    {
                        UserAddressId = 99999,
                        DeliveryInstructions =
                            "Call when you arrive."
                    });

            var response =
                await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.Contains(
                "delivery address",
                responseBody,
                StringComparison.OrdinalIgnoreCase);

            Assert.Contains(
                "not found",
                responseBody,
                StringComparison.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------
        // DIRECT ORDER CREATION IS DISABLED
        // ---------------------------------------------------------

        [Fact]
        public async Task
            DirectOrderCreationEndpoint_IsNotAvailable()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "/api/Order",
                    customerToken,
                    new
                    {
                        Items = new[]
                        {
                            new
                            {
                                FoodItemId = 10001,
                                Quantity = 1
                            }
                        }
                    });

            var response =
                await _client.SendAsync(request);

            Assert.Equal(
                HttpStatusCode.MethodNotAllowed,
                response.StatusCode);
        }

        // ---------------------------------------------------------
        // GET CUSTOMER'S ORDERS
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetMyOrders_WithCustomerToken_ReturnsCustomerOrder()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var createdOrderId =
                await CreateOrderAsync(
                    customerToken,
                    quantity: 1);

            using var getRequest =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    "/api/Order/my-orders",
                    customerToken);

            var getResponse =
                await _client.SendAsync(getRequest);

            var getBody =
                await getResponse.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            using var getJson =
                JsonDocument.Parse(getBody);

            var matchingOrder =
                getJson.RootElement
                    .EnumerateArray()
                    .FirstOrDefault(order =>
                        order.GetProperty("id").GetInt32()
                        == createdOrderId);

            Assert.Equal(
                createdOrderId,
                matchingOrder
                    .GetProperty("id")
                    .GetInt32());

            Assert.Equal(
                "12, MG Road",
                matchingOrder
                    .GetProperty("deliveryAddressLine1")
                    .GetString());
        }

        // ---------------------------------------------------------
        // GET CUSTOMER'S ORDER BY ID
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetMyOrderById_WhenOrderBelongsToCustomer_ReturnsOk()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var createdOrderId =
                await CreateOrderAsync(
                    customerToken,
                    quantity: 3);

            using var getRequest =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    $"/api/Order/my-orders/{createdOrderId}",
                    customerToken);

            var getResponse =
                await _client.SendAsync(getRequest);

            var getBody =
                await getResponse.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            using var getJson =
                JsonDocument.Parse(getBody);

            var order =
                getJson.RootElement;

            Assert.Equal(
                createdOrderId,
                order.GetProperty("id").GetInt32());

            Assert.Equal(
                540m,
                order
                    .GetProperty("totalAmount")
                    .GetDecimal());

            Assert.Equal(
                "Bengaluru",
                order
                    .GetProperty("deliveryCity")
                    .GetString());

            Assert.Equal(
                "560001",
                order
                    .GetProperty("deliveryPostalCode")
                    .GetString());
        }

        // ---------------------------------------------------------
        // ORDER OWNERSHIP PROTECTION
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetMyOrderById_WhenOrderBelongsToAnotherCustomer_ReturnsNotFound()
        {
            var firstCustomerToken =
                await CreateCustomerTokenAsync();

            var secondCustomerToken =
                await CreateCustomerTokenAsync();

            var createdOrderId =
                await CreateOrderAsync(
                    firstCustomerToken,
                    quantity: 1);

            using var getRequest =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    $"/api/Order/my-orders/{createdOrderId}",
                    secondCustomerToken);

            var getResponse =
                await _client.SendAsync(getRequest);

            Assert.Equal(
                HttpStatusCode.NotFound,
                getResponse.StatusCode);
        }

        // ---------------------------------------------------------
        // ADMIN GET ALL ORDERS
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetAllOrders_WithAdminToken_ReturnsCreatedOrder()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var createdOrderId =
                await CreateOrderAsync(
                    customerToken,
                    quantity: 1);

            var adminToken =
                await CreateAdminTokenAsync();

            using var getRequest =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    "/api/Order",
                    adminToken);

            var getResponse =
                await _client.SendAsync(getRequest);

            var getBody =
                await getResponse.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            using var getJson =
                JsonDocument.Parse(getBody);

            var matchingOrder =
                getJson.RootElement
                    .EnumerateArray()
                    .FirstOrDefault(order =>
                        order.GetProperty("id").GetInt32()
                        == createdOrderId);

            Assert.Equal(
                createdOrderId,
                matchingOrder
                    .GetProperty("id")
                    .GetInt32());

            Assert.Equal(
                "Order Test Customer",
                matchingOrder
                    .GetProperty("deliveryRecipientName")
                    .GetString());
        }

        // ---------------------------------------------------------
        // ADMIN GET ORDER BY ID
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetOrderById_WithAdminToken_ReturnsOrder()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var createdOrderId =
                await CreateOrderAsync(
                    customerToken,
                    quantity: 2);

            var adminToken =
                await CreateAdminTokenAsync();

            using var getRequest =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    $"/api/Order/{createdOrderId}",
                    adminToken);

            var getResponse =
                await _client.SendAsync(getRequest);

            var getBody =
                await getResponse.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            using var getJson =
                JsonDocument.Parse(getBody);

            var order =
                getJson.RootElement;

            Assert.Equal(
                createdOrderId,
                order.GetProperty("id").GetInt32());

            Assert.Equal(
                360m,
                order
                    .GetProperty("totalAmount")
                    .GetDecimal());

            Assert.Equal(
                "12, MG Road",
                order
                    .GetProperty("deliveryAddressLine1")
                    .GetString());
        }

        // ---------------------------------------------------------
        // UPDATE ORDER STATUS
        // ---------------------------------------------------------

        [Fact]
        public async Task
            UpdateOrderStatus_WithAdminToken_ReturnsUpdatedOrder()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderId =
                await CreateOrderAsync(
                    customerToken,
                    quantity: 1);

            var adminToken =
                await CreateAdminTokenAsync();

            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Put,
                    $"/api/Order/{orderId}/status",
                    adminToken,
                    new
                    {
                        OrderStatus = "Confirmed"
                    });

            var response =
                await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            var order =
                jsonDocument.RootElement;

            Assert.Equal(
                orderId,
                order.GetProperty("id").GetInt32());

            Assert.Equal(
                "Confirmed",
                order
                    .GetProperty("orderStatus")
                    .GetString());

            Assert.Equal(
                "12, MG Road",
                order
                    .GetProperty("deliveryAddressLine1")
                    .GetString());
        }

        [Fact]
        public async Task
            UpdateOrderStatus_WithInvalidStatus_ReturnsBadRequest()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderId =
                await CreateOrderAsync(
                    customerToken,
                    quantity: 1);

            var adminToken =
                await CreateAdminTokenAsync();

            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Put,
                    $"/api/Order/{orderId}/status",
                    adminToken,
                    new
                    {
                        OrderStatus = "Invalid Status"
                    });

            var response =
                await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.Contains(
                "OrderStatus",
                responseBody);
        }

        [Fact]
        public async Task
            UpdateOrderStatus_WhenOrderDoesNotExist_ReturnsNotFound()
        {
            var adminToken =
                await CreateAdminTokenAsync();

            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Put,
                    "/api/Order/99999/status",
                    adminToken,
                    new
                    {
                        OrderStatus = "Confirmed"
                    });

            var response =
                await _client.SendAsync(request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        // ---------------------------------------------------------
        // PERMANENT ORDER DELETION IS DISABLED
        // ---------------------------------------------------------

        [Fact]
        public async Task
            DeleteOrderEndpoint_IsNotAvailableAndOrderRemains()
        {
            var customerToken =
                await CreateCustomerTokenAsync();

            var orderId =
                await CreateOrderAsync(
                    customerToken,
                    quantity: 1);

            var adminToken =
                await CreateAdminTokenAsync();

            using var deleteRequest =
                CreateAuthorizedRequest(
                    HttpMethod.Delete,
                    $"/api/Order/{orderId}",
                    adminToken);

            var deleteResponse =
                await _client.SendAsync(deleteRequest);

            Assert.Equal(
                HttpStatusCode.MethodNotAllowed,
                deleteResponse.StatusCode);

            using var getRequest =
                CreateAuthorizedRequest(
                    HttpMethod.Get,
                    $"/api/Order/{orderId}",
                    adminToken);

            var getResponse =
                await _client.SendAsync(getRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);
        }

        // ---------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------

        // Registers and logs in a unique customer.
        private async Task<string>
            CreateCustomerTokenAsync()
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

            var loginResponse =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    new
                    {
                        Email = customerEmail,
                        Password = customerPassword
                    });

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

        // Logs in the integration-test Admin account.
        private async Task<string>
            CreateAdminTokenAsync()
        {
            var loginResponse =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    new
                    {
                        Email =
                            "integration-admin@test.local",

                        Password =
                            "IntegrationTest@12345"
                    });

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

        // Saves a delivery address and returns its generated ID.
        private async Task<int>
            CreateAddressAsync(
                string customerToken)
        {
            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "/api/UserAddresses",
                    customerToken,
                    new
                    {
                        AddressLabel = "Home",
                        RecipientName =
                            "Order Test Customer",
                        RecipientPhone =
                            "9876543213",
                        AddressLine1 =
                            "12, MG Road",
                        AddressLine2 =
                            "Apartment 4B",
                        Landmark =
                            "Near Metro Station",
                        City = "Bengaluru",
                        State = "Karnataka",
                        PostalCode = "560001",
                        IsDefault = true
                    });

            var response =
                await _client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            return jsonDocument.RootElement
                .GetProperty("id")
                .GetInt32();
        }

        // Adds the seeded test food item to the customer's cart.
        private async Task AddCartItemAsync(
            string customerToken,
            int quantity)
        {
            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "/api/Cart/items",
                    customerToken,
                    new
                    {
                        FoodItemId = 10001,
                        Quantity = quantity
                    });

            var response =
                await _client.SendAsync(request);

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }

        // Completes the production checkout flow.
        private async Task<HttpResponseMessage>
            CheckoutCartAsync(
                string customerToken,
                int quantity)
        {
            var addressId =
                await CreateAddressAsync(customerToken);

            await AddCartItemAsync(
                customerToken,
                quantity);

            using var request =
                CreateAuthorizedRequest(
                    HttpMethod.Post,
                    "/api/Cart/checkout",
                    customerToken,
                    new
                    {
                        UserAddressId = addressId,
                        DeliveryInstructions =
                            "  Call when you arrive.  "
                    });

            return await _client.SendAsync(request);
        }

        // Creates an order and returns its generated ID.
        private async Task<int> CreateOrderAsync(
            string customerToken,
            int quantity)
        {
            using var response =
                await CheckoutCartAsync(
                    customerToken,
                    quantity);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            using var jsonDocument =
                JsonDocument.Parse(responseBody);

            return jsonDocument.RootElement
                .GetProperty("order")
                .GetProperty("id")
                .GetInt32();
        }

        // Creates an HTTP request containing a Bearer token.
        private static HttpRequestMessage
            CreateAuthorizedRequest(
                HttpMethod method,
                string requestUri,
                string token,
                object? body = null)
        {
            var request =
                new HttpRequestMessage(
                    method,
                    requestUri);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            if (body != null)
            {
                request.Content =
                    JsonContent.Create(body);
            }

            return request;
        }
    }
}