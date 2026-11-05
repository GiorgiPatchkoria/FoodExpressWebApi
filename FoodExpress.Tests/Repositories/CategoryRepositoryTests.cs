using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using FoodExpress.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Tests.Repositories
{
    public class CategoryRepositoryTests
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
            var repository = new CategoryRepository(context);
            var item = new Category { Name = "Pizzas" };

            // Act
            await repository.AddAsync(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Assert
            var saved = await context.Categories.SingleAsync();
            Assert.Equal(item.Id, saved.Id);
            Assert.Equal(item.Name, saved.Name);
        }

        [Fact]
        public async Task FindAsync_ReturnsSavedEntity()
        {
            // Arrange
            using var context = CreateContext();
            var item = new Category { Name = "Pizzas" };
            context.Categories.Add(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repository = new CategoryRepository(context);

            // Act
            var result = await repository.FindAsync(item.Id, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(item.Name, result.Name);
        }
    }
}