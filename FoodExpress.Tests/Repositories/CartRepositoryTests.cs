using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using FoodExpress.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Tests.Repositories
{
    public class CartRepositoryTests
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
            var repository = new CartRepository(context);
            var item = new Cart { UserId = 1 };

            // Act
            await repository.AddAsync(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Assert
            var saved = await context.Carts.SingleAsync();
            Assert.Equal(item.Id, saved.Id);
        }

        [Fact]
        public async Task FindAsync_ReturnsSavedEntity()
        {
            // Arrange
            using var context = CreateContext();
            var item = new Cart { UserId = 1 };
            context.Carts.Add(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repository = new CartRepository(context);

            // Act
            var result = await repository.FindByUserAsync(1, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
        }
    }
}