using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using FoodExpress.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Tests.Repositories
{
    public class OrderRepositoryTests
    {
        private FoodDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FoodDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FoodDbContext(options);
        }

        [Fact]
        public async Task AddAsync_SavesEntity()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new OrderRepository(context);
            var item = new Order { UserId = 1, Address = new Address { StreetAddress = "Main Street", City = "Tbilisi", CountryCode = "GE" } };

            // Act
            await repository.AddAsync(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Assert
            var saved = await context.Orders.Include(order => order.Address).SingleAsync();
            Assert.Equal(item.Id, saved.Id);
            Assert.Equal(item.Address.StreetAddress, saved.Address.StreetAddress);
            Assert.Equal(item.Address.City, saved.Address.City);
            Assert.Equal(item.Address.CountryCode, saved.Address.CountryCode);
        }

        [Fact]
        public async Task FindAsync_ReturnsSavedEntity()
        {
            // Arrange
            using var context = CreateContext();
            var item = new Order { UserId = 1, Address = new Address { StreetAddress = "Main Street", City = "Tbilisi", CountryCode = "GE" } };
            context.Orders.Add(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repository = new OrderRepository(context);

            // Act
            var result = await repository.FindAsync(item.Id, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(item.Address.StreetAddress, result.Address.StreetAddress);
            Assert.Equal(item.Address.City, result.Address.City);
            Assert.Equal(item.Address.CountryCode, result.Address.CountryCode);
        }
    }
}