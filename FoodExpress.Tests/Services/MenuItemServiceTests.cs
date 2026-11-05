using AutoMapper;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Services.Profiles;
using FoodExpress.Services.Services.Implementations;
using FoodExpress.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FoodExpress.Tests.Services
{
    public class MenuItemServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();
        private readonly Mock<ICurrentUser> _current = new Mock<ICurrentUser>();
        private readonly Mock<IMenuItemRepository> _menu = new Mock<IMenuItemRepository>();
        private readonly Mock<ICategoryRepository> _categories = new Mock<ICategoryRepository>();
        private readonly Mock<IRestaurantRepository> _restaurants = new Mock<IRestaurantRepository>();

        private MenuItemService CreateService()
        {
            _current.SetupGet(user => user.UserId).Returns(1);
            _current.SetupGet(user => user.IsAdmin).Returns(true);
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(UserProfile).Assembly), NullLoggerFactory.Instance);
            return new MenuItemService(_menu.Object, _categories.Object, _restaurants.Object, _unitOfWork.Object, new RequestValidator(), config.CreateMapper(), _current.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidDish_AddsAndSaves()
        {
            // Arrange
            var service = CreateService();
            _restaurants.Setup(repo => repo.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Restaurant { Id = 1 });
            _categories.Setup(repo => repo.CategoryExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _menu.Setup(repo => repo.AddAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await service.CreateAsync(new CreateMenuItemDTO { RestaurantId = 1, CategoryId = 1, Name = " Soup ", Price = 10m });

            // Assert
            Assert.Equal("Soup", result.Name);
            Assert.Equal(10m, result.Price);
            _menu.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()), Times.Once());
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_NegativePriceIsRejected()
        {
            // Arrange
            var service = CreateService();
            var request = new CreateMenuItemDTO { RestaurantId = 1, CategoryId = 1, Name = "Soup", Price = -1m };

            // Act
            var error = await Assert.ThrowsAsync<ServiceException>(() => service.CreateAsync(request));

            // Assert
            Assert.Equal(ServiceError.Validation, error.Error);
            _menu.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()), Times.Never());
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}