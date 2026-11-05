using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using FoodExpress.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Tests.Repositories
{
    public class MenuItemRepositoryTests
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
            var repository = new MenuItemRepository(context);
            var item = new MenuItem { Name = "Soup", Price = 10m, Restaurant = new Restaurant { Name = "Cafe" }, Category = new Category { Name = "Main Dishes" } };

            // Act
            await repository.AddAsync(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Assert
            var saved = await context.MenuItems.Include(item => item.Restaurant).Include(item => item.Category).SingleAsync();
            Assert.Equal(item.Id, saved.Id);
            Assert.Equal(item.Restaurant.Name, saved.Restaurant.Name);
            Assert.Equal(item.Category.Name, saved.Category.Name);
            Assert.Equal(item.Name, saved.Name);
            Assert.Equal(item.Price, saved.Price);
        }

        [Fact]
        public async Task FindAsync_ReturnsSavedEntity()
        {
            // Arrange
            using var context = CreateContext();
            var item = new MenuItem { Name = "Soup", Price = 10m, Restaurant = new Restaurant { Name = "Cafe" }, Category = new Category { Name = "Main Dishes" } };
            context.MenuItems.Add(item);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repository = new MenuItemRepository(context);

            // Act
            var result = await repository.FindAsync(item.Id, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(item.Restaurant.Name, result.Restaurant.Name);
            Assert.Equal(item.Name, result.Name);
            Assert.Equal(item.Price, result.Price);
        }
    }
}