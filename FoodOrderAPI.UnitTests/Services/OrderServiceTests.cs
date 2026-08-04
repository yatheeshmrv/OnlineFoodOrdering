using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;
using FoodOrderAPI.Services;
using Moq;
using Xunit;

namespace FoodOrderAPI.UnitTests.Services
{
    public class OrderServiceTests
    {
        // Creates a commonly used sample order containing
        // items and an immutable delivery-address snapshot.
        private static Order CreateSampleOrder(
            int id = 1,
            string userId = "user-1",
            string status = "Pending")
        {
            return new Order
            {
                Id = id,
                CustomerName = "Yatheesh",
                CustomerPhone = "9876543210",
                TotalAmount = 360m,
                OrderStatus = status,

                OrderDate = new DateTime(
                    2026,
                    7,
                    28,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),

                UserId = userId,

                DeliveryRecipientName = "Yatheesh",
                DeliveryPhone = "9876543210",
                DeliveryAddressLine1 = "12, MG Road",
                DeliveryAddressLine2 = "Apartment 4B",
                DeliveryLandmark = "Near Metro Station",
                DeliveryCity = "Bengaluru",
                DeliveryState = "Karnataka",
                DeliveryPostalCode = "560001",

                DeliveryInstructions =
                    "Call when you arrive.",

                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        FoodItemId = 10,

                        FoodItem = new FoodItem
                        {
                            Id = 10,
                            Name = "Paneer Fried Rice",
                            Price = 180m,
                            IsAvailable = true
                        },

                        Quantity = 2,
                        UnitPrice = 180m
                    }
                }
            };
        }

        // ---------------------------------------------------------
        // GET ALL ORDERS
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetAllOrdersAsync_WhenOrdersExist_ReturnsAllOrderDtos()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetAllOrdersAsync())
                .ReturnsAsync(
                    new List<Order>
                    {
                        CreateSampleOrder()
                    });

            var service = new OrderService(
                orderRepositoryMock.Object);

            var result =
                await service.GetAllOrdersAsync();

            Assert.Single(result);

            var order = result[0];

            Assert.Equal(1, order.Id);
            Assert.Equal("Yatheesh", order.CustomerName);
            Assert.Equal(360m, order.TotalAmount);
            Assert.Equal("Pending", order.OrderStatus);

            Assert.Equal(
                "Yatheesh",
                order.DeliveryRecipientName);

            Assert.Equal(
                "9876543210",
                order.DeliveryPhone);

            Assert.Equal(
                "12, MG Road",
                order.DeliveryAddressLine1);

            Assert.Equal(
                "Apartment 4B",
                order.DeliveryAddressLine2);

            Assert.Equal(
                "Near Metro Station",
                order.DeliveryLandmark);

            Assert.Equal(
                "Bengaluru",
                order.DeliveryCity);

            Assert.Equal(
                "Karnataka",
                order.DeliveryState);

            Assert.Equal(
                "560001",
                order.DeliveryPostalCode);

            Assert.Equal(
                "Call when you arrive.",
                order.DeliveryInstructions);

            Assert.Single(order.Items);

            Assert.Equal(
                "Paneer Fried Rice",
                order.Items[0].FoodItemName);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetAllOrdersAsync(),
                Times.Once);
        }

        // ---------------------------------------------------------
        // GET ORDER BY ID
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetOrderByIdAsync_WhenOrderExists_ReturnsOrderDto()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAsync(5))
                .ReturnsAsync(
                    CreateSampleOrder(id: 5));

            var service = new OrderService(
                orderRepositoryMock.Object);

            var result =
                await service.GetOrderByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Yatheesh", result.CustomerName);
            Assert.Equal(360m, result.TotalAmount);
            Assert.Equal("Bengaluru", result.DeliveryCity);
            Assert.Equal("560001", result.DeliveryPostalCode);
            Assert.Single(result.Items);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrderByIdAsync(5),
                Times.Once);
        }

        [Fact]
        public async Task
            GetOrderByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAsync(999))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(
                orderRepositoryMock.Object);

            var result =
                await service.GetOrderByIdAsync(999);

            Assert.Null(result);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrderByIdAsync(999),
                Times.Once);
        }

        // ---------------------------------------------------------
        // GET CUSTOMER ORDERS
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetMyOrdersAsync_WhenUserIdIsValid_ReturnsUserOrders()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrdersByUserIdAsync(
                        "user-1"))
                .ReturnsAsync(
                    new List<Order>
                    {
                        CreateSampleOrder()
                    });

            var service = new OrderService(
                orderRepositoryMock.Object);

            var result =
                (await service.GetMyOrdersAsync(
                    "user-1"))
                .ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);

            Assert.Equal(
                "12, MG Road",
                result[0].DeliveryAddressLine1);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrdersByUserIdAsync(
                        "user-1"),
                Times.Once);
        }

        [Fact]
        public async Task
            GetMyOrdersAsync_WhenUserIdIsMissing_ThrowsArgumentException()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetMyOrdersAsync(" "));

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrdersByUserIdAsync(
                        It.IsAny<string>()),
                Times.Never);
        }

        // ---------------------------------------------------------
        // GET CUSTOMER ORDER BY ID
        // ---------------------------------------------------------

        [Fact]
        public async Task
            GetMyOrderByIdAsync_WhenOrderBelongsToUser_ReturnsOrder()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository
                        .GetOrderByIdAndUserIdAsync(
                            5,
                            "user-1"))
                .ReturnsAsync(
                    CreateSampleOrder(id: 5));

            var service = new OrderService(
                orderRepositoryMock.Object);

            var result =
                await service.GetMyOrderByIdAsync(
                    5,
                    "user-1");

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Bengaluru", result.DeliveryCity);

            orderRepositoryMock.Verify(
                repository =>
                    repository
                        .GetOrderByIdAndUserIdAsync(
                            5,
                            "user-1"),
                Times.Once);
        }

        [Fact]
        public async Task
            GetMyOrderByIdAsync_WhenOrderDoesNotBelongToUser_ReturnsNull()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository
                        .GetOrderByIdAndUserIdAsync(
                            5,
                            "user-2"))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(
                orderRepositoryMock.Object);

            var result =
                await service.GetMyOrderByIdAsync(
                    5,
                    "user-2");

            Assert.Null(result);
        }

        [Fact]
        public async Task
            GetMyOrderByIdAsync_WhenIdIsInvalid_ThrowsException()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object);

            await Assert.ThrowsAsync<
                ArgumentOutOfRangeException>(
                () => service.GetMyOrderByIdAsync(
                    0,
                    "user-1"));

            orderRepositoryMock.Verify(
                repository =>
                    repository
                        .GetOrderByIdAndUserIdAsync(
                            It.IsAny<int>(),
                            It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task
            GetMyOrderByIdAsync_WhenUserIdIsMissing_ThrowsException()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetMyOrderByIdAsync(
                    5,
                    " "));

            orderRepositoryMock.Verify(
                repository =>
                    repository
                        .GetOrderByIdAndUserIdAsync(
                            It.IsAny<int>(),
                            It.IsAny<string>()),
                Times.Never);
        }

        // ---------------------------------------------------------
        // UPDATE ORDER STATUS
        // ---------------------------------------------------------

        [Fact]
        public async Task
            UpdateOrderStatusAsync_WhenStatusIsValid_ReturnsUpdatedOrder()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.UpdateOrderStatusAsync(
                        1,
                        "Out for Delivery"))
                .ReturnsAsync(
                    CreateSampleOrder(
                        status: "Out for Delivery"));

            var service = new OrderService(
                orderRepositoryMock.Object);

            var result =
                await service.UpdateOrderStatusAsync(
                    1,
                    new UpdateOrderStatusDto
                    {
                        OrderStatus =
                            "  out for delivery  "
                    });

            Assert.NotNull(result);

            Assert.Equal(
                "Out for Delivery",
                result.OrderStatus);

            Assert.Equal(
                "12, MG Road",
                result.DeliveryAddressLine1);

            orderRepositoryMock.Verify(
                repository =>
                    repository.UpdateOrderStatusAsync(
                        1,
                        "Out for Delivery"),
                Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Shipped")]
        public async Task
            UpdateOrderStatusAsync_WhenStatusIsInvalid_ThrowsException(
                string invalidStatus)
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object);

            var statusDto = new UpdateOrderStatusDto
            {
                OrderStatus = invalidStatus
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdateOrderStatusAsync(
                    1,
                    statusDto));

            orderRepositoryMock.Verify(
                repository =>
                    repository.UpdateOrderStatusAsync(
                        It.IsAny<int>(),
                        It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task
            UpdateOrderStatusAsync_WhenDtoIsNull_ThrowsArgumentNullException()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object);

            await Assert.ThrowsAsync<
                ArgumentNullException>(
                () => service.UpdateOrderStatusAsync(
                    1,
                    null!));

            orderRepositoryMock.Verify(
                repository =>
                    repository.UpdateOrderStatusAsync(
                        It.IsAny<int>(),
                        It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task
            UpdateOrderStatusAsync_WhenOrderDoesNotExist_ReturnsNull()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.UpdateOrderStatusAsync(
                        999,
                        "Delivered"))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(
                orderRepositoryMock.Object);

            var result =
                await service.UpdateOrderStatusAsync(
                    999,
                    new UpdateOrderStatusDto
                    {
                        OrderStatus = "Delivered"
                    });

            Assert.Null(result);

            orderRepositoryMock.Verify(
                repository =>
                    repository.UpdateOrderStatusAsync(
                        999,
                        "Delivered"),
                Times.Once);
        }
    }
}