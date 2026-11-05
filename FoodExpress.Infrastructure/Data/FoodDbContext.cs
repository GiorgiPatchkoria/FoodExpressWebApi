using FoodExpress.Core.Models.Entities;
using FoodExpress.Core.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FoodExpress.Infrastructure.Data
{
    public class FoodDbContext : IdentityUserContext<ApplicationUser, int>
    {
        public FoodDbContext(DbContextOptions<FoodDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Address>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.StreetAddress).HasMaxLength(250).IsRequired();
                b.Property(x => x.City).HasMaxLength(100).IsRequired();
                b.Property(x => x.PostalCode).HasMaxLength(20);
                b.Property(x => x.CountryCode).HasMaxLength(2).IsUnicode(false).IsRequired();
                b.HasOne(x => x.User).WithMany(x => x.Addresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Cart>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.UserId).IsUnique();
                b.HasOne(x => x.User).WithOne(x => x.Cart).HasForeignKey<Cart>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(b =>
            {
                b.ToTable(t => t.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0"));
                b.HasKey(x => new { x.CartId, x.MenuItemId });
                b.HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.MenuItem).WithMany(x => x.CartItems).HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Category>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(100).IsRequired();
                b.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<MenuItem>(b =>
            {
                b.Property(x => x.ImageUrl).HasMaxLength(500);
                b.ToTable(t => t.HasCheckConstraint("CK_MenuItems_Price", "[Price] >= 0"));
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(150).IsRequired();
                b.Property(x => x.Description).HasMaxLength(2000);
                b.Property(x => x.Price).HasPrecision(18, 2);
                b.HasIndex(x => new { x.RestaurantId, x.Name }).IsUnique();
                b.HasOne(x => x.Restaurant).WithMany(x => x.MenuItems).HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.NoAction);
                b.HasOne(x => x.Category).WithMany(x => x.MenuItems).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Order>(b =>
            {
                b.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Orders_TotalPrice", "[TotalPrice] >= 0");
                    t.HasCheckConstraint("CK_Orders_Status", "[Status] IN (0, 1, 2, 3, 4, 5)");
                });
                b.HasKey(x => x.Id);
                b.Property(x => x.Status).HasConversion<int>();
                b.Property(x => x.TotalPrice).HasPrecision(18, 2);
                b.HasOne(x => x.Address).WithMany(x => x.Orders).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.NoAction);
                b.HasIndex(x => new { x.UserId, x.CreatedAt });
                b.HasIndex(x => new { x.RestaurantId, x.Status, x.CreatedAt });
                b.HasOne(x => x.User).WithMany(x => x.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
                b.HasOne(x => x.Restaurant).WithMany(x => x.Orders).HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<OrderItem>(b =>
            {
                b.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0");
                    t.HasCheckConstraint("CK_OrderItems_UnitPrice", "[UnitPrice] >= 0");
                });
                b.HasKey(x => new { x.OrderId, x.MenuItemId });
                b.Property(x => x.ItemName).HasMaxLength(150).IsRequired();
                b.Property(x => x.UnitPrice).HasPrecision(18, 2);
                b.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.MenuItem).WithMany(x => x.OrderItems).HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Restaurant>(b =>
            {
                b.Property(x => x.ImageUrl).HasMaxLength(500);
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(150).IsRequired();
                b.Property(x => x.StreetAddress).HasMaxLength(250).IsRequired();
                b.Property(x => x.City).HasMaxLength(100).IsRequired();
                b.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
            });

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.UserName).HasMaxLength(50).IsRequired();
                b.HasIndex(x => x.UserName).IsUnique();
                b.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
                b.Property(x => x.RoleId).HasDefaultValue(FoodExpress.Core.Models.Identity.Roles.CustomerId);
                b.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.NoAction);
                b.Property(x => x.Email).HasMaxLength(256).IsRequired();
                b.HasIndex(x => x.Email).IsUnique();
                b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
                b.Property(x => x.PhoneNumber).HasMaxLength(30);
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(50).IsRequired();
                b.HasIndex(x => x.Name).IsUnique();
                b.HasData(new Role { Id = 1, Name = "Customer" }, new Role { Id = 2, Name = "Admin" });
            });

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Main dishes" },
                new Category { Id = 2, Name = "Sides" },
                new Category { Id = 3, Name = "Desserts" },
                new Category { Id = 4, Name = "Drinks" });

        }
    }
}