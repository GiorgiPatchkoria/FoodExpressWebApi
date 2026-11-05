using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using FoodExpress.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Tests.Repositories
{
    public class RestaurantRepositoryTests
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
            var repository = new RestaurantRepository(context);
            var item = new Restaurant { Name = "Cafe", StreetAddress = "Main Street", City = "Tbilisi", PhoneNumber = "555123456" };

            // Act
            await repository.AddAsync(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Assert
            var saved = await context.Restaurants.SingleAsync();
            Assert.Equal(item.Id, saved.Id);
            Assert.Equal(item.Name, saved.Name);
            Assert.Equal(item.StreetAddress, saved.StreetAddress);
            Assert.Equal(item.City, saved.City);
            Assert.Equal(item.PhoneNumber, saved.PhoneNumber);
        }

        [Fact]
        public async Task FindAsync_ReturnsSavedEntity()
        {
            // Arrange
            using var context = CreateContext();
            var item = new Restaurant { Name = "Cafe", StreetAddress = "Main Street", City = "Tbilisi", PhoneNumber = "555123456" };
            context.Restaurants.Add(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repository = new RestaurantRepository(context);

            // Act
            var result = await repository.FindAsync(item.Id, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(item.Name, result.Name);
            Assert.Equal(item.StreetAddress, result.StreetAddress);
            Assert.Equal(item.City, result.City);
            Assert.Equal(item.PhoneNumber, result.PhoneNumber);
        }
    }
}