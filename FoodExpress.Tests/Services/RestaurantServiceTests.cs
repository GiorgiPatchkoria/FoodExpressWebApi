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
    public class RestaurantServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();
        private readonly Mock<ICurrentUser> _current = new Mock<ICurrentUser>();
        private readonly Mock<IRestaurantRepository> _restaurants = new Mock<IRestaurantRepository>();

        private RestaurantService CreateService()
        {
            _current.SetupGet(user => user.UserId).Returns(1);
            _current.SetupGet(user => user.IsAdmin).Returns(true);
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(UserProfile).Assembly), NullLoggerFactory.Instance);
            return new RestaurantService(_restaurants.Object, _unitOfWork.Object, new RequestValidator(), config.CreateMapper(), _current.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidRestaurant_AddsAndSaves()
        {
            // Arrange
            var service = CreateService();
            _restaurants.Setup(repo => repo.AddAsync(It.IsAny<Restaurant>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await service.CreateAsync(new CreateRestaurantDTO { Name = " Cafe ", StreetAddress = "Main Street", City = "Tbilisi", PhoneNumber = "+995555123456" });

            // Assert
            Assert.Equal("Cafe", result.Name);
            _restaurants.Verify(repo => repo.AddAsync(It.IsAny<Restaurant>(), It.IsAny<CancellationToken>()), Times.Once());
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_CustomerIsRejected()
        {
            // Arrange
            var service = CreateService();
            _current.SetupGet(user => user.IsAdmin).Returns(false);

            // Act
            var error = await Assert.ThrowsAsync<ServiceException>(() => service.CreateAsync(new CreateRestaurantDTO()));

            // Assert
            Assert.Equal(ServiceError.Forbidden, error.Error);
            _restaurants.Verify(repo => repo.AddAsync(It.IsAny<Restaurant>(), It.IsAny<CancellationToken>()), Times.Never());
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}