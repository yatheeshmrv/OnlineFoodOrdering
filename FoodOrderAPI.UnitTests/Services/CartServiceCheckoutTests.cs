using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace FoodOrderAPI.UnitTests.Services
{
    // Contains focused unit tests for shopping-cart checkout.
    public class CartServiceCheckoutTests
    {
        // Creates the UserManager mock required by CartService.
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

        // Creates a registered customer with a complete profile.
        private static ApplicationUser CreateValidUser()
        {
            return new ApplicationUser
            {
                Id = "user-1",
                FullName = "  Yatheesh  ",
                PhoneNumber = "  9876543210  "
            };
        }

        // Creates a cart containing one item.
        private static Cart CreateCartWithItem(
            int foodItemId = 10,
            int quantity = 2)
        {
            return new Cart
            {
                Id = 25,
                UserId = "user-1",
                CartItems = new List<CartItem>
                {
                    new CartItem
                    {
                        Id = 50,
                        CartId = 25,
                        FoodItemId = foodItemId,
                        Quantity = quantity
                    }
                }
            };
        }

        // ---------------------------------------------------------
        // MISSING USER ID
        // ---------------------------------------------------------

        [Fact]
        public async Task CheckoutAsync_WhenUserIdIsMissing_ReturnsFailure()
        {
            var cartRepositoryMock =
                new Mock<ICartRepository>();

            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            var service = new CartService(
                cartRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                orderRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.CheckoutAsync(" ");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Authenticated user ID is missing.",
                result.Message);
            Assert.Null(result.Order);

            userManagerMock.Verify(
                manager =>
                    manager.FindByIdAsync(
                        It.IsAny<string>()),
                Times.Never);

            cartRepositoryMock.Verify(
                repository =>
                    repository.GetOrCreateCartAsync(
                        It.IsAny<string>()),
                Times.Never);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderFromCartAsync(
                        It.IsAny<Order>(),
                        It.IsAny<int>()),
                Times.Never);
        }

        // ---------------------------------------------------------
        // USER NOT FOUND
        // ---------------------------------------------------------

        [Fact]
        public async Task CheckoutAsync_WhenUserIsNotFound_ReturnsFailure()
        {
            var cartRepositoryMock =
                new Mock<ICartRepository>();

            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync((ApplicationUser?)null);

            var service = new CartService(
                cartRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                orderRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.CheckoutAsync("user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Authenticated user account was not found.",
                result.Message);
            Assert.Null(result.Order);

            cartRepositoryMock.Verify(
                repository =>
                    repository.GetOrCreateCartAsync(
                        It.IsAny<string>()),
                Times.Never);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderFromCartAsync(
                        It.IsAny<Order>(),
                        It.IsAny<int>()),
                Times.Never);
        }

        // ---------------------------------------------------------
        // INCOMPLETE CUSTOMER PROFILE
        // ---------------------------------------------------------

        [Fact]
        public async Task CheckoutAsync_WhenProfileIsIncomplete_ReturnsFailure()
        {
            var cartRepositoryMock =
                new Mock<ICartRepository>();

            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(
                    new ApplicationUser
                    {
                        Id = "user-1",
                        FullName = " ",
                        PhoneNumber = "9876543210"
                    });

            var service = new CartService(
                cartRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                orderRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.CheckoutAsync("user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Customer profile is incomplete. " +
                "Full name and phone number are required.",
                result.Message);
            Assert.Null(result.Order);

            cartRepositoryMock.Verify(
                repository =>
                    repository.GetOrCreateCartAsync(
                        It.IsAny<string>()),
                Times.Never);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderFromCartAsync(
                        It.IsAny<Order>(),
                        It.IsAny<int>()),
                Times.Never);
        }

        // ---------------------------------------------------------
        // EMPTY CART
        // ---------------------------------------------------------

        [Fact]
        public async Task CheckoutAsync_WhenCartIsEmpty_ReturnsFailure()
        {
            var cartRepositoryMock =
                new Mock<ICartRepository>();

            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(CreateValidUser());

            cartRepositoryMock
                .Setup(repository =>
                    repository.GetOrCreateCartAsync(
                        "user-1"))
                .ReturnsAsync(
                    new Cart
                    {
                        Id = 25,
                        UserId = "user-1",
                        CartItems = new List<CartItem>()
                    });

            var service = new CartService(
                cartRepositoryMock.Object,
                new Mock<IFoodItemRepository>().Object,
                orderRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.CheckoutAsync("user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "The cart is empty. Add at least one item " +
                "before checkout.",
                result.Message);
            Assert.Null(result.Order);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderFromCartAsync(
                        It.IsAny<Order>(),
                        It.IsAny<int>()),
                Times.Never);
        }

        // ---------------------------------------------------------
        // FOOD ITEM NOT FOUND
        // ---------------------------------------------------------

        [Fact]
        public async Task CheckoutAsync_WhenFoodItemDoesNotExist_ReturnsFailure()
        {
            var cartRepositoryMock =
                new Mock<ICartRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(CreateValidUser());

            cartRepositoryMock
                .Setup(repository =>
                    repository.GetOrCreateCartAsync(
                        "user-1"))
                .ReturnsAsync(
                    CreateCartWithItem(
                        foodItemId: 999,
                        quantity: 1));

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(999))
                .ReturnsAsync((FoodItem?)null);

            var service = new CartService(
                cartRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                orderRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.CheckoutAsync("user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Food item 999 was not found.",
                result.Message);
            Assert.Null(result.Order);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderFromCartAsync(
                        It.IsAny<Order>(),
                        It.IsAny<int>()),
                Times.Never);

            cartRepositoryMock.Verify(
                repository =>
                    repository.ClearCartAsync(
                        It.IsAny<string>()),
                Times.Never);
        }

        // ---------------------------------------------------------
        // FOOD ITEM UNAVAILABLE
        // ---------------------------------------------------------

        [Fact]
        public async Task CheckoutAsync_WhenFoodItemIsUnavailable_ReturnsFailure()
        {
            var cartRepositoryMock =
                new Mock<ICartRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(CreateValidUser());

            cartRepositoryMock
                .Setup(repository =>
                    repository.GetOrCreateCartAsync(
                        "user-1"))
                .ReturnsAsync(CreateCartWithItem());

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

            var service = new CartService(
                cartRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                orderRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.CheckoutAsync("user-1");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "Paneer Fried Rice is currently unavailable.",
                result.Message);
            Assert.Null(result.Order);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderFromCartAsync(
                        It.IsAny<Order>(),
                        It.IsAny<int>()),
                Times.Never);

            cartRepositoryMock.Verify(
                repository =>
                    repository.ClearCartAsync(
                        It.IsAny<string>()),
                Times.Never);
        }

        // ---------------------------------------------------------
        // SUCCESSFUL CHECKOUT
        // ---------------------------------------------------------

        [Fact]
        public async Task CheckoutAsync_WhenCartIsValid_CreatesOrderAndClearsCartAtomically()
        {
            var cartRepositoryMock =
                new Mock<ICartRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var userManagerMock =
                CreateUserManagerMock();

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

            var cart = new Cart
            {
                Id = 25,
                UserId = "user-1",
                CartItems = new List<CartItem>
                {
                    new CartItem
                    {
                        Id = 50,
                        CartId = 25,
                        FoodItemId = 10,
                        Quantity = 2
                    },
                    new CartItem
                    {
                        Id = 51,
                        CartId = 25,
                        FoodItemId = 11,
                        Quantity = 1
                    }
                }
            };

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(CreateValidUser());

            cartRepositoryMock
                .Setup(repository =>
                    repository.GetOrCreateCartAsync(
                        "user-1"))
                .ReturnsAsync(cart);

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
                    repository.CreateOrderFromCartAsync(
                        It.IsAny<Order>(),
                        25))
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
                OrderDate = new DateTime(
                    2026,
                    7,
                    29,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
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

            var service = new CartService(
                cartRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                orderRepositoryMock.Object,
                userManagerMock.Object);

            var result =
                await service.CheckoutAsync("user-1");

            Assert.True(result.IsSuccess);
            Assert.Equal(
                "Order created successfully.",
                result.Message);

            Assert.NotNull(result.Order);
            Assert.Equal(100, result.Order.Id);
            Assert.Equal("Yatheesh", result.Order.CustomerName);
            Assert.Equal(
                "9876543210",
                result.Order.CustomerPhone);
            Assert.Equal(480m, result.Order.TotalAmount);
            Assert.Equal("Pending", result.Order.OrderStatus);
            Assert.Equal(2, result.Order.Items.Count);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderFromCartAsync(
                        It.Is<Order>(order =>
                            order.CustomerName == "Yatheesh" &&
                            order.CustomerPhone ==
                                "9876543210" &&
                            order.UserId == "user-1" &&
                            order.OrderStatus == "Pending" &&
                            order.TotalAmount == 480m &&
                            order.OrderItems.Count == 2 &&
                            order.OrderItems.Any(item =>
                                item.FoodItemId == 10 &&
                                item.Quantity == 2 &&
                                item.UnitPrice == 180m) &&
                            order.OrderItems.Any(item =>
                                item.FoodItemId == 11 &&
                                item.Quantity == 1 &&
                                item.UnitPrice == 120m)),
                        25),
                Times.Once);

            // Checkout must use the combined repository method.
            // It must not clear the cart through a separate save.
            cartRepositoryMock.Verify(
                repository =>
                    repository.ClearCartAsync(
                        It.IsAny<string>()),
                Times.Never);

            orderRepositoryMock.Verify(
                repository =>
                    repository.CreateOrderAsync(
                        It.IsAny<Order>()),
                Times.Never);

            orderRepositoryMock.Verify(
                repository =>
                    repository.GetOrderByIdAsync(100),
                Times.Once);
        }

        // ---------------------------------------------------------
        // SAVED ORDER CANNOT BE RELOADED
        // ---------------------------------------------------------

        [Fact]
        public async Task CheckoutAsync_WhenCreatedOrderCannotBeReloaded_ThrowsException()
        {
            var cartRepositoryMock =
                new Mock<ICartRepository>();

            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var orderRepositoryMock =
                new Mock<IOrderRepository>();

            var userManagerMock =
                CreateUserManagerMock();

            userManagerMock
                .Setup(manager =>
                    manager.FindByIdAsync("user-1"))
                .ReturnsAsync(CreateValidUser());

            cartRepositoryMock
                .Setup(repository =>
                    repository.GetOrCreateCartAsync(
                        "user-1"))
                .ReturnsAsync(CreateCartWithItem());

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
                    repository.CreateOrderFromCartAsync(
                        It.IsAny<Order>(),
                        25))
                .ReturnsAsync(
                    new Order
                    {
                        Id = 100
                    });

            orderRepositoryMock
                .Setup(repository =>
                    repository.GetOrderByIdAsync(100))
                .ReturnsAsync((Order?)null);

            var service = new CartService(
                cartRepositoryMock.Object,
                foodItemRepositoryMock.Object,
                orderRepositoryMock.Object,
                userManagerMock.Object);

            var exception =
                await Assert.ThrowsAsync<
                    InvalidOperationException>(
                    () => service.CheckoutAsync(
                        "user-1"));

            Assert.Equal(
                "The order was created but could not be retrieved.",
                exception.Message);
        }
    }
}