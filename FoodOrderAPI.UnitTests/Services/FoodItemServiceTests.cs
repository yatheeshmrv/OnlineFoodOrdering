using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Repositories;
using FoodOrderAPI.Services;
using Moq;
using Xunit;

namespace FoodOrderAPI.UnitTests.Services
{
    public class FoodItemServiceTests
    {
        // Tests the retrieval of a food item by its ID when the item exists in the repository.
        [Fact]
        public async Task GetFoodItemByIdAsync_WhenItemExists_ReturnsFoodItemDto()
        {
            // Arrange: creates fake repository objects.
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            // Creates the food item that the fake repository will return.
            var foodItem = new FoodItem
            {
                Id = 1,
                Name = "Paneer Fried Rice",
                Description = "Fried rice with paneer",
                Price = 180m,
                FoodCategoryId = 7,
                FoodCategory = new FoodCategory
                {
                    Id = 7,
                    CategoryName = "Healthy Meals"
                },
                IsAvailable = true
            };

            // Configures the fake repository response.
            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(1))
                .ReturnsAsync(foodItem);

            // Creates the service using the fake repositories.
            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act: calls the method being tested.
            var result =
                await foodItemService.GetFoodItemByIdAsync(1);

            // Assert: verifies the returned DTO.
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Paneer Fried Rice", result.Name);
            Assert.Equal(180m, result.Price);
            Assert.Equal(7, result.FoodCategoryId);
            Assert.Equal("Healthy Meals", result.FoodCategoryName);
            Assert.True(result.IsAvailable);

            // Confirms that the repository method was called once.
            foodItemRepositoryMock.Verify(
                repository =>
                    repository.GetFoodItemByIdAsync(1),
                Times.Once);
        }

        // Tests the retrieval of a food item by its ID when the item does not exist in the repository.
        [Fact]
        public async Task GetFoodItemByIdAsync_WhenItemDoesNotExist_ReturnsNull()
        {
            // Arrange: creates fake repositories.
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            // Configures the repository to return null.
            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(999))
                .ReturnsAsync((FoodItem?)null);

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var result =
                await foodItemService.GetFoodItemByIdAsync(999);

            // Assert
            Assert.Null(result);

            foodItemRepositoryMock.Verify(
                repository =>
                    repository.GetFoodItemByIdAsync(999),
                Times.Once);
        }

        // Tests the retrieval of all food items when items exist in the repository.
        [Fact]
        public async Task GetAllFoodItemsAsync_WhenItemsExist_ReturnsAllItems()
        {
            // Arrange
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            var foodItems = new List<FoodItem>
    {
        new FoodItem
        {
            Id = 1,
            Name = "Paneer Fried Rice",
            Description = "Fried rice with paneer",
            Price = 180m,
            FoodCategoryId = 7,
            FoodCategory = new FoodCategory
            {
                Id = 7,
                CategoryName = "Healthy Meals"
            },
            IsAvailable = true
        },
        new FoodItem
        {
            Id = 2,
            Name = "Vegetable Salad",
            Description = "Fresh vegetable salad",
            Price = 120m,
            FoodCategoryId = 7,
            FoodCategory = new FoodCategory
            {
                Id = 7,
                CategoryName = "Healthy Meals"
            },
            IsAvailable = true
        }
    };

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetAllFoodItemsAsync())
                .ReturnsAsync(foodItems);

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var result =
                await foodItemService.GetAllFoodItemsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Paneer Fried Rice", result[0].Name);
            Assert.Equal("Vegetable Salad", result[1].Name);
            Assert.All(
                result,
                item => Assert.Equal(
                    "Healthy Meals",
                    item.FoodCategoryName));

            foodItemRepositoryMock.Verify(
                repository =>
                    repository.GetAllFoodItemsAsync(),
                Times.Once);
        }

        // Tests the retrieval of paged food items when items exist in the repository.
        [Fact]
        public async Task GetPagedFoodItemsAsync_WhenItemsExist_ReturnsPagedResponse()
        {
            // Arrange
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            var queryParameters =
                new FoodItemQueryParametersDto
                {
                    Search = "Paneer",
                    CategoryId = 7,
                    PageNumber = 2,
                    PageSize = 2
                };

            var foodItems = new List<FoodItem>
    {
        new FoodItem
        {
            Id = 3,
            Name = "Paneer Fried Rice",
            Description = "Fried rice with paneer",
            Price = 180m,
            FoodCategoryId = 7,
            FoodCategory = new FoodCategory
            {
                Id = 7,
                CategoryName = "Healthy Meals"
            },
            IsAvailable = true
        }
    };

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetPagedFoodItemsAsync(
                        "Paneer",
                        7,
                        2,
                        2))
                .ReturnsAsync((foodItems, 3));

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var result =
                await foodItemService.GetPagedFoodItemsAsync(
                    queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Paneer Fried Rice", result.Items[0].Name);
            Assert.Equal("Healthy Meals", result.Items[0].FoodCategoryName);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.TotalPages);

            foodItemRepositoryMock.Verify(
                repository =>
                    repository.GetPagedFoodItemsAsync(
                        "Paneer",
                        7,
                        2,
                        2),
                Times.Once);
        }

        // Tests the addition of a new food item when the provided data is valid.
        [Fact]
        public async Task AddFoodItemAsync_WhenDataIsValid_ReturnsCreatedFoodItem()
        {
            // Arrange
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            var foodCategory = new FoodCategory
            {
                Id = 7,
                CategoryName = "Healthy Meals",
                IsActive = true
            };

            var requestDto = new FoodItemDto
            {
                Name = "  Paneer Fried Rice  ",
                Description = "  Fried rice with paneer  ",
                Price = 180m,
                FoodCategoryId = 7,
                IsAvailable = true
            };

            var addedFoodItem = new FoodItem
            {
                Id = 10,
                Name = "Paneer Fried Rice",
                Description = "Fried rice with paneer",
                Price = 180m,
                FoodCategoryId = 7,
                IsAvailable = true
            };

            var savedFoodItem = new FoodItem
            {
                Id = 10,
                Name = "Paneer Fried Rice",
                Description = "Fried rice with paneer",
                Price = 180m,
                FoodCategoryId = 7,
                FoodCategory = foodCategory,
                IsAvailable = true
            };

            // Confirms that the selected category exists.
            foodCategoryRepositoryMock
                .Setup(repository =>
                    repository.GetFoodCategoryByIdAsync(7))
                .ReturnsAsync(foodCategory);

            // Simulates saving the food item.
            foodItemRepositoryMock
                .Setup(repository =>
                    repository.AddFoodItemAsync(
                        It.IsAny<FoodItem>()))
                .ReturnsAsync(addedFoodItem);

            // Simulates retrieving the saved item with its category.
            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(10))
                .ReturnsAsync(savedFoodItem);

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var result =
                await foodItemService.AddFoodItemAsync(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Paneer Fried Rice", result.Name);
            Assert.Equal("Fried rice with paneer", result.Description);
            Assert.Equal(180m, result.Price);
            Assert.Equal(7, result.FoodCategoryId);
            Assert.Equal("Healthy Meals", result.FoodCategoryName);
            Assert.True(result.IsAvailable);

            // Confirms that trimmed values were sent for saving.
            foodItemRepositoryMock.Verify(
                repository =>
                    repository.AddFoodItemAsync(
                        It.Is<FoodItem>(item =>
                            item.Name == "Paneer Fried Rice" &&
                            item.Description ==
                                "Fried rice with paneer" &&
                            item.Price == 180m &&
                            item.FoodCategoryId == 7 &&
                            item.IsAvailable)),
                Times.Once);

            foodCategoryRepositoryMock.Verify(
                repository =>
                    repository.GetFoodCategoryByIdAsync(7),
                Times.Once);

            foodItemRepositoryMock.Verify(
                repository =>
                    repository.GetFoodItemByIdAsync(10),
                Times.Once);
        }

        // Tests the addition of a new food item when the provided data is invalid.
        [Theory]
        [InlineData(
    "",
    180,
    7,
    "Food item name is required.")]
        [InlineData(
    "Paneer Fried Rice",
    0,
    7,
    "Food item price must be greater than 0.")]
        [InlineData(
    "Paneer Fried Rice",
    180,
    0,
    "Valid food category is required.")]
        public async Task AddFoodItemAsync_WhenBasicDataIsInvalid_ThrowsArgumentException(
    string name,
    int price,
    int categoryId,
    string expectedMessage)
        {
            // Arrange
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            var requestDto = new FoodItemDto
            {
                Name = name,
                Description = "Test description",
                Price = price,
                FoodCategoryId = categoryId,
                IsAvailable = true
            };

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => foodItemService.AddFoodItemAsync(
                        requestDto));

            // Assert
            Assert.Equal(expectedMessage, exception.Message);

            // Invalid data must not be sent to the repositories.
            foodItemRepositoryMock.Verify(
                repository =>
                    repository.AddFoodItemAsync(
                        It.IsAny<FoodItem>()),
                Times.Never);

            foodCategoryRepositoryMock.Verify(
                repository =>
                    repository.GetFoodCategoryByIdAsync(
                        It.IsAny<int>()),
                Times.Never);
        }

        // Tests the addition of a new food item when the specified category does not exist in the repository.
        [Fact]
        public async Task AddFoodItemAsync_WhenCategoryDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            var requestDto = new FoodItemDto
            {
                Name = "Paneer Fried Rice",
                Description = "Fried rice with paneer",
                Price = 180m,
                FoodCategoryId = 999,
                IsAvailable = true
            };

            // Simulates a category that does not exist.
            foodCategoryRepositoryMock
                .Setup(repository =>
                    repository.GetFoodCategoryByIdAsync(999))
                .ReturnsAsync((FoodCategory?)null);

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => foodItemService.AddFoodItemAsync(
                        requestDto));

            // Assert
            Assert.Equal(
                "Food category does not exist.",
                exception.Message);

            foodCategoryRepositoryMock.Verify(
                repository =>
                    repository.GetFoodCategoryByIdAsync(999),
                Times.Once);

            // The invalid item must not be saved.
            foodItemRepositoryMock.Verify(
                repository =>
                    repository.AddFoodItemAsync(
                        It.IsAny<FoodItem>()),
                Times.Never);
        }

        // Tests the update of an existing food item when the item exists in the repository.
        [Fact]
        public async Task UpdateFoodItemAsync_WhenItemExists_ReturnsUpdatedFoodItem()
        {
            // Arrange
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            var foodCategory = new FoodCategory
            {
                Id = 7,
                CategoryName = "Healthy Meals",
                IsActive = true
            };

            var requestDto = new FoodItemDto
            {
                Name = "  Updated Paneer Rice  ",
                Description = "  Updated description  ",
                Price = 200m,
                FoodCategoryId = 7,
                IsAvailable = true
            };

            var updatedFoodItem = new FoodItem
            {
                Id = 10,
                Name = "Updated Paneer Rice",
                Description = "Updated description",
                Price = 200m,
                FoodCategoryId = 7,
                IsAvailable = true
            };

            var savedFoodItem = new FoodItem
            {
                Id = 10,
                Name = "Updated Paneer Rice",
                Description = "Updated description",
                Price = 200m,
                FoodCategoryId = 7,
                FoodCategory = foodCategory,
                IsAvailable = true
            };

            foodCategoryRepositoryMock
                .Setup(repository =>
                    repository.GetFoodCategoryByIdAsync(7))
                .ReturnsAsync(foodCategory);

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.UpdateFoodItemAsync(
                        10,
                        It.IsAny<FoodItem>()))
                .ReturnsAsync(updatedFoodItem);

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.GetFoodItemByIdAsync(10))
                .ReturnsAsync(savedFoodItem);

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var result =
                await foodItemService.UpdateFoodItemAsync(
                    10,
                    requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Updated Paneer Rice", result.Name);
            Assert.Equal("Updated description", result.Description);
            Assert.Equal(200m, result.Price);
            Assert.Equal(7, result.FoodCategoryId);
            Assert.Equal("Healthy Meals", result.FoodCategoryName);
            Assert.True(result.IsAvailable);

            // Confirms that trimmed values were sent for updating.
            foodItemRepositoryMock.Verify(
                repository =>
                    repository.UpdateFoodItemAsync(
                        10,
                        It.Is<FoodItem>(item =>
                            item.Name == "Updated Paneer Rice" &&
                            item.Description ==
                                "Updated description" &&
                            item.Price == 200m &&
                            item.FoodCategoryId == 7 &&
                            item.IsAvailable)),
                Times.Once);

            foodCategoryRepositoryMock.Verify(
                repository =>
                    repository.GetFoodCategoryByIdAsync(7),
                Times.Once);

            foodItemRepositoryMock.Verify(
                repository =>
                    repository.GetFoodItemByIdAsync(10),
                Times.Once);
        }

        // Tests the scenario where the food item to be updated does not exist in the repository.
        [Fact]
        public async Task UpdateFoodItemAsync_WhenItemDoesNotExist_ReturnsNull()
        {
            // Arrange
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            var foodCategory = new FoodCategory
            {
                Id = 7,
                CategoryName = "Healthy Meals",
                IsActive = true
            };

            var requestDto = new FoodItemDto
            {
                Name = "Paneer Fried Rice",
                Description = "Fried rice with paneer",
                Price = 180m,
                FoodCategoryId = 7,
                IsAvailable = true
            };

            foodCategoryRepositoryMock
                .Setup(repository =>
                    repository.GetFoodCategoryByIdAsync(7))
                .ReturnsAsync(foodCategory);

            // Simulates an update for an ID that does not exist.
            foodItemRepositoryMock
                .Setup(repository =>
                    repository.UpdateFoodItemAsync(
                        999,
                        It.IsAny<FoodItem>()))
                .ReturnsAsync((FoodItem?)null);

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var result =
                await foodItemService.UpdateFoodItemAsync(
                    999,
                    requestDto);

            // Assert
            Assert.Null(result);

            foodCategoryRepositoryMock.Verify(
                repository =>
                    repository.GetFoodCategoryByIdAsync(7),
                Times.Once);

            foodItemRepositoryMock.Verify(
                repository =>
                    repository.UpdateFoodItemAsync(
                        999,
                        It.IsAny<FoodItem>()),
                Times.Once);

            // Retrieval must not happen when the update returns null.
            foodItemRepositoryMock.Verify(
                repository =>
                    repository.GetFoodItemByIdAsync(
                        It.IsAny<int>()),
                Times.Never);
        }

        // Tests the deletion of a food item and verifies that the service returns the same result as the repository.
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteFoodItemAsync_ReturnsRepositoryResult(
    bool repositoryResult)
        {
            // Arrange
            var foodItemRepositoryMock =
                new Mock<IFoodItemRepository>();

            var foodCategoryRepositoryMock =
                new Mock<IFoodCategoryRepository>();

            foodItemRepositoryMock
                .Setup(repository =>
                    repository.DeleteFoodItemAsync(10))
                .ReturnsAsync(repositoryResult);

            var foodItemService = new FoodItemService(
                foodItemRepositoryMock.Object,
                foodCategoryRepositoryMock.Object);

            // Act
            var result =
                await foodItemService.DeleteFoodItemAsync(10);

            // Assert
            Assert.Equal(repositoryResult, result);

            foodItemRepositoryMock.Verify(
                repository =>
                    repository.DeleteFoodItemAsync(10),
                Times.Once);
        }
    }
}