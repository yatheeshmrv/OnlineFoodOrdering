using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace FoodOrderAPI.UnitTests.Services
{
    public class OrderServiceTests
    {
        // Creates the UserManager mock required by OrderService.
        private static Mock<UserManager<ApplicationUser>>
            CreateUserManagerMock()
        {
            var userStoreMock =
                new Mock<IUserStore<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);
        }

        // Creates a commonly used sample order.
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
        public async Task GetAllOrdersAsync_WhenOrdersExist_ReturnsAllOrderDtos()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            var orders = new List<Order>
            {
                CreateSampleOrder()
            };

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetAllOrdersAsync())
                .ReturnsAsync(orders);

            var service = new OrderService(
                orderRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.GetAllOrdersAsync();

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Yatheesh", result[0].CustomerName);
            Assert.Equal(360m, result[0].TotalAmount);
            Assert.Equal("Pending", result[0].OrderStatus);

            Assert.Single(result[0].Items);
            Assert.Equal(
                "Paneer Fried Rice",
                result[0].Items[0].FoodItemName);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetAllOrdersAsync(),
                Times.Once);
        }

        // ---------------------------------------------------------
        // GET ORDER BY ID
        // ---------------------------------------------------------

        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderExists_ReturnsOrderDto()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAsync(5))
                .ReturnsAsync(
                    CreateSampleOrder(id: 5));

            var service = new OrderService(
                orderRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.GetOrderByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Yatheesh", result.CustomerName);
            Assert.Equal(360m, result.TotalAmount);
            Assert.Single(result.Items);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrderByIdAsync(5),
                Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAsync(999))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(
                orderRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                userManagerMock.Object);

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
        public async Task GetMyOrdersAsync_WhenUserIdIsValid_ReturnsUserOrders()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var userManagerMock =
                CreateUserManagerMock();

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
                orderRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                (await service.GetMyOrdersAsync(
                    "user-1")).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrdersByUserIdAsync(
                        "user-1"),
                Times.Once);
        }

        [Fact]
        public async Task GetMyOrdersAsync_WhenUserIdIsMissing_ThrowsArgumentException()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

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
        public async Task GetMyOrderByIdAsync_WhenOrderBelongsToUser_ReturnsOrder()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAndUserIdAsync(
                        5,
                        "user-1"))
                .ReturnsAsync(
                    CreateSampleOrder(id: 5));

            var service = new OrderService(
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

            var result =
                await service.GetMyOrderByIdAsync(
                    5,
                    "user-1");

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrderByIdAndUserIdAsync(
                        5,
                        "user-1"),
                Times.Once);
        }

        [Fact]
        public async Task GetMyOrderByIdAsync_WhenOrderDoesNotBelongToUser_ReturnsNull()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAndUserIdAsync(
                        5,
                        "user-2"))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

            var result =
                await service.GetMyOrderByIdAsync(
                    5,
                    "user-2");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetMyOrderByIdAsync_WhenIdIsInvalid_ThrowsException()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

            await Assert.ThrowsAsync<
                ArgumentOutOfRangeException>(
                () => service.GetMyOrderByIdAsync(
                    0,
                    "user-1"));

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrderByIdAndUserIdAsync(
                        It.IsAny<int>(),
                        It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task GetMyOrderByIdAsync_WhenUserIdIsMissing_ThrowsException()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetMyOrderByIdAsync(
                    5,
                    " "));
        }

        // ---------------------------------------------------------
        // CREATE ORDER
        // ---------------------------------------------------------

        [Fact]
        public async Task CreateOrderAsync_WhenUserIdIsMissing_ReturnsFailure()
        {
            var userManagerMock =
                CreateUserManagerMock();

            var service = new OrderService(
                new Mock<IOrderRepository>().Object,
                new Mock<IFoodItemRepository>().Object,
                userManagerMock.Object);

            var request = new CreateOrderDto
            {
                Items = new List<OrderItemRequestDto>()
            };

            var result =
                await service.CreateOrderAsync(
                    request,
                    " ");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Authenticated user ID is missing.",
                result.Message);

            userManagerMock.Verify(
                manager =>
                    manager.FindByIdAsync(
                        It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenUserIsNotFound_ReturnsFailure()
        {
            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync((ApplicationUser?)null);

            var service = new OrderService(
                new Mock<IOrderRepository>().Object,
                new Mock<IFoodItemRepository>().Object,
                userManagerMock.Object);

            var result =
                await service.CreateOrderAsync(
                    new CreateOrderDto
                    {
                        Items =
                            new List<OrderItemRequestDto>()
                    },
                    "user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Authenticated user account was not found.",
                result.Message);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenProfileIsIncomplete_ReturnsFailure()
        {
            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(
                    new ApplicationUser
                    {
                        Id = "user-1",
                        FullName = "",
                        PhoneNumber = "9876543210"
                    });

            var service = new OrderService(
                new Mock<IOrderRepository>().Object,
                new Mock<IFoodItemRepository>().Object,
                userManagerMock.Object);

            var result =
                await service.CreateOrderAsync(
                    new CreateOrderDto
                    {
                        Items =
                            new List<OrderItemRequestDto>()
                    },
                    "user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Customer profile is incomplete. " +
                "Full name and phone number are required.",
                result.Message);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenFoodItemDoesNotExist_ReturnsFailure()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(
                    new ApplicationUser
                    {
                        Id = "user-1",
                        FullName = "Yatheesh",
                        PhoneNumber = "9876543210"
                    });

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(999))
                .ReturnsAsync((FoodItem?)null);

            var service = new OrderService(
                orderRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                userManagerMock.Object);

            var request = new CreateOrderDto
            {
                Items = new List<OrderItemRequestDto>
                {
                    new OrderItemRequestDto
                    {
                        FoodItemId = 999,
                        Quantity = 1
                    }
                }
            };

            var result =
                await service.CreateOrderAsync(
                    request,
                    "user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Food item 999 was not found.",
                result.Message);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderAsync(
                        It.IsAny<Order>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenFoodItemIsUnavailable_ReturnsFailure()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(
                    new ApplicationUser
                    {
                        Id = "user-1",
                        FullName = "Yatheesh",
                        PhoneNumber = "9876543210"
                    });

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(10))
                .ReturnsAsync(
                    new FoodItem
                    {
                        Id = 10,
                        Name = "Paneer Fried Rice",
                        Price = 180m,
                        IsAvailable = false
                    });

            var service = new OrderService(
                orderRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                userManagerMock.Object);

            var request = new CreateOrderDto
            {
                Items = new List<OrderItemRequestDto>
                {
                    new OrderItemRequestDto
                    {
                        FoodItemId = 10,
                        Quantity = 1
                    }
                }
            };

            var result =
                await service.CreateOrderAsync(
                    request,
                    "user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Paneer Fried Rice is currently unavailable.",
                result.Message);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderAsync(
                        It.IsAny<Order>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenRequestIsValid_CreatesOrder()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            var user = new ApplicationUser
            {
                Id = "user-1",
                FullName = "  Yatheesh  ",
                PhoneNumber = "  9876543210  "
            };

            var firstFoodItem = new FoodItem
            {
                Id = 10,
                Name = "Paneer Fried Rice",
                Price = 180m,
                IsAvailable = true
            };

            var secondFoodItem = new FoodItem
            {
                Id = 11,
                Name = "Vegetable Salad",
                Price = 120m,
                IsAvailable = true
            };

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(10))
                .ReturnsAsync(firstFoodItem);

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(11))
                .ReturnsAsync(secondFoodItem);

            orderRepositoryMock
                .Setup(repository =>
                    repository.CreateOrderAsync(
                        It.IsAny<Order>()))
                .ReturnsAsync(
                    new Order
                    {
                        Id = 100
                    });

            var savedOrder = new Order
            {
                Id = 100,
                CustomerName = "Yatheesh",
                CustomerPhone = "9876543210",
                UserId = "user-1",
                TotalAmount = 480m,
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        FoodItemId = 10,
                        FoodItem = firstFoodItem,
                        Quantity = 2,
                        UnitPrice = 180m
                    },
                    new OrderItem
                    {
                        FoodItemId = 11,
                        FoodItem = secondFoodItem,
                        Quantity = 1,
                        UnitPrice = 120m
                    }
                }
            };

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAsync(100))
                .ReturnsAsync(savedOrder);

            var service = new OrderService(
                orderRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                userManagerMock.Object);

            var request = new CreateOrderDto
            {
                Items = new List<OrderItemRequestDto>
                {
                    new OrderItemRequestDto
                    {
                        FoodItemId = 10,
                        Quantity = 2
                    },
                    new OrderItemRequestDto
                    {
                        FoodItemId = 11,
                        Quantity = 1
                    }
                }
            };

            var result =
                await service.CreateOrderAsync(
                    request,
                    "user-1");

            Assert.True(result.IsSuccess);
            Assert.Equal(
                "Order created successfully.",
                result.Message);

            Assert.NotNull(result.Order);
            Assert.Equal(100, result.Order.Id);
            Assert.Equal(480m, result.Order.TotalAmount);
            Assert.Equal("Pending", result.Order.OrderStatus);
            Assert.Equal(2, result.Order.Items.Count);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderAsync(
                        It.Is<Order>(order =>
                            order.CustomerName ==
                                "Yatheesh" &&
                            order.CustomerPhone ==
                                "9876543210" &&
                            order.UserId == "user-1" &&
                            order.OrderStatus ==
                                "Pending" &&
                            order.TotalAmount ==
                                480m &&
                            order.OrderItems.Count ==
                                2)),
                Times.Once);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrderByIdAsync(100),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenCreatedOrderCannotBeReloaded_ThrowsException()
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(
                    new ApplicationUser
                    {
                        Id = "user-1",
                        FullName = "Yatheesh",
                        PhoneNumber = "9876543210"
                    });

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(10))
                .ReturnsAsync(
                    new FoodItem
                    {
                        Id = 10,
                        Name = "Paneer Fried Rice",
                        Price = 180m,
                        IsAvailable = true
                    });

            orderRepositoryMock
                .Setup(repository =>
                    repository.CreateOrderAsync(
                        It.IsAny<Order>()))
                .ReturnsAsync(
                    new Order
                    {
                        Id = 100
                    });

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAsync(100))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(
                orderRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                userManagerMock.Object);

            var request = new CreateOrderDto
            {
                Items = new List<OrderItemRequestDto>
                {
                    new OrderItemRequestDto
                    {
                        FoodItemId = 10,
                        Quantity = 1
                    }
                }
            };

            var exception =
                await Assert.ThrowsAsync<
                    InvalidOperationException>(
                    () => service.CreateOrderAsync(
                        request,
                        "user-1"));

            Assert.Equal(
                "The order was created but could not be retrieved.",
                exception.Message);
        }

        // ---------------------------------------------------------
        // UPDATE ORDER STATUS
        // ---------------------------------------------------------

        [Fact]
        public async Task UpdateOrderStatusAsync_WhenStatusIsValid_ReturnsUpdatedOrder()
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
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

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
        public async Task UpdateOrderStatusAsync_WhenStatusIsInvalid_ThrowsException(
            string invalidStatus)
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var service = new OrderService(
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

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
        public async Task UpdateOrderStatusAsync_WhenOrderDoesNotExist_ReturnsNull()
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
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

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

        // ---------------------------------------------------------
        // DELETE ORDER
        // ---------------------------------------------------------

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteOrderAsync_ReturnsRepositoryResult(
            bool repositoryResult)
        {
            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            orderRepositoryMock
                .Setup(repository =>
                    repository.DeleteOrderAsync(1))
                .ReturnsAsync(repositoryResult);

            var service = new OrderService(
                orderRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                CreateUserManagerMock().Object);

            var result =
                await service.DeleteOrderAsync(1);

            Assert.Equal(repositoryResult, result);

            orderRepositoryMock.Verify(
                repository =>
                    repository.DeleteOrderAsync(1),
                Times.Once);
        }
    }
}