using AutoMapper;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Services.Profiles;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Services.Services.Implementations;
using FoodExpress.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FoodExpress.Tests.Services
{
    public class CartServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();
        private readonly Mock<ICurrentUser> _current = new Mock<ICurrentUser>();
        private readonly Mock<ICartRepository> _carts = new Mock<ICartRepository>();
        private readonly Mock<IMenuItemRepository> _menu = new Mock<IMenuItemRepository>();
        private readonly Mock<IUserService> _users = new Mock<IUserService>();

        private CartService CreateService()
        {
            _current.SetupGet(user => user.UserId).Returns(1);
            _current.SetupGet(user => user.IsAdmin).Returns(true);
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(UserProfile).Assembly), NullLoggerFactory.Instance);
            return new CartService(_carts.Object, _menu.Object, _unitOfWork.Object, _users.Object, new RequestValidator(), config.CreateMapper());
        }


        [Fact]
        public async Task AddItemAsync_CalculatesTotalAndSaves()
        {
            // Arrange
            var service = CreateService();
            _menu.Setup(repo => repo.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new MenuItem { Id = 1, Name = "Soup", Price = 10m, IsAvailable = true, Restaurant = new Restaurant { IsActive = true } });
            _carts.Setup(repo => repo.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await service.AddItemAsync(1, new AddCartItemDTO { MenuItemId = 1, Quantity = 3 });

            // Assert
            Assert.Equal(30m, result.Subtotal);
            Assert.Equal(3, Assert.Single(result.Items).Quantity);
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task AddItemAsync_UnavailableDishIsRejected()
        {
            // Arrange
            var service = CreateService();
            _menu.Setup(repo => repo.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new MenuItem { Id = 1, IsAvailable = false });

            // Act
            var error = await Assert.ThrowsAsync<ServiceException>(() => service.AddItemAsync(1, new AddCartItemDTO { MenuItemId = 1, Quantity = 1 }));

            // Assert
            Assert.Equal(ServiceError.Conflict, error.Error);
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}