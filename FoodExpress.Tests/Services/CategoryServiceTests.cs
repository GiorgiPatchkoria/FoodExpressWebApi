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
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();
        private readonly Mock<ICurrentUser> _current = new Mock<ICurrentUser>();
        private readonly Mock<ICategoryRepository> _categories = new Mock<ICategoryRepository>();

        private CategoryService CreateService()
        {
            _current.SetupGet(user => user.UserId).Returns(1);
            _current.SetupGet(user => user.IsAdmin).Returns(true);
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(UserProfile).Assembly), NullLoggerFactory.Instance);
            return new CategoryService(_categories.Object, _unitOfWork.Object, new RequestValidator(), config.CreateMapper(), _current.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidCategory_AddsAndSaves()
        {
            // Arrange
            var service = CreateService();
            _categories.Setup(repo => repo.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await service.CreateAsync(new CreateCategoryDTO { Name = "Soups" });

            // Assert
            Assert.Equal("Soups", result.Name);
            _categories.Verify(repo => repo.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once());
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_DuplicateNameIsRejected()
        {
            // Arrange
            var service = CreateService();
            _categories.Setup(repo => repo.NameExistsAsync("Soups", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var error = await Assert.ThrowsAsync<ServiceException>(() => service.CreateAsync(new CreateCategoryDTO { Name = "Soups" }));

            // Assert
            Assert.Equal(ServiceError.Conflict, error.Error);
            _categories.Verify(repo => repo.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never());
            _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}