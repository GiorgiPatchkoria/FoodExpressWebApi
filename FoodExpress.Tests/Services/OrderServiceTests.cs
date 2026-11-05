using AutoMapper;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Core.Enums;
using FoodExpress.Services.Profiles;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Services.Services.Implementations;
using FoodExpress.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FoodExpress.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();
        private readonly Mock<ICurrentUser> _current = new Mock<ICurrentUser>();
        private readonly Mock<IOrderRepository> _orders = new Mock<IOrderRepository>();
        private readonly Mock<ICartRepository> _carts = new Mock<ICartRepository>();
        private readonly Mock<IAddressRepository> _addresses = new Mock<IAddressRepository>();
        private readonly Mock<IUserService> _users = new Mock<IUserService>();

        private OrderService CreateService()
        {
            _current.SetupGet(user => user.UserId).Returns(1);
            _current.SetupGet(user => user.IsAdmin).Returns(true);
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(UserProfile).Assembly), NullLoggerFactory.Instance);
            return new OrderService(_orders.Object, _carts.Object, _addresses.Object, _unitOfWork.Object, _users.Object, new RequestValidator(), _current.Object, config.CreateMapper());
        }

        [Fact]
        public async Task CancelAsync_CreatedOrderCanBeCancelled()
        {
            // Arrange
            var service = CreateService();
            var order = new Order { Id = 1, UserId = 1, Status = OrderStatus.Created, Address = new Address() };
            _orders.Setup(repo => repo.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var result = await service.CancelAsync(1);

            // Assert
            Assert.Equal(OrderStatus.Cancelled, result.Order.Status);
            Assert.Equal(OrderStatus.Cancelled, order.Status);
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CancelAsync_PreparingOrderIsRejected()
        {
            // Arrange
            var service = CreateService();
            var order = new Order { Id = 1, UserId = 1, Status = OrderStatus.Preparing };
            _orders.Setup(repo => repo.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

            // Act
            var error = await Assert.ThrowsAsync<ServiceException>(() => service.CancelAsync(1));

            // Assert
            Assert.Equal(ServiceError.Conflict, error.Error);
            Assert.Equal(OrderStatus.Preparing, order.Status);
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}