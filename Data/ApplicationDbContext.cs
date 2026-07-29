using FoodOrderAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Data
{
    // Main Entity Framework database context.
    // Connects the application's models with SQL Server tables.
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        // Receives the database configuration registered in Program.cs.
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Represents the FoodCategories table.
        public DbSet<FoodCategory> FoodCategories { get; set; }

        // Represents the FoodItems table.
        public DbSet<FoodItem> FoodItems { get; set; }

        // Represents the Orders table.
        public DbSet<Order> Orders { get; set; }

        // Represents the OrderItems table.
        public DbSet<OrderItem> OrderItems { get; set; }

        // Represents the Carts table.
        public DbSet<Cart> Carts { get; set; }

        // Represents the CartItems table.
        public DbSet<CartItem> CartItems { get; set; }

        // Configures relationships, decimal columns and seed data.
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            // Applies the default ASP.NET Core Identity configuration.
            // This must be called before adding our custom configurations.
            base.OnModelCreating(modelBuilder);

            // ---------------------------------------------------------
            // APPLICATION USER AND ORDER RELATIONSHIP
            // ---------------------------------------------------------

            // One registered user can place many orders.
            // Order.UserId is the foreign key connected to AspNetUsers.Id.
            modelBuilder.Entity<Order>()
                .HasOne(order => order.User)
                .WithMany()
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------------------------------
            // ORDER AND ORDER ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // One Order can contain many OrderItems.
            // OrderItem.OrderId is the foreign key.
            // Deleting an order also deletes its order items.
            modelBuilder.Entity<OrderItem>()
                .HasOne(orderItem => orderItem.Order)
                .WithMany(order => order.OrderItems)
                .HasForeignKey(orderItem => orderItem.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------------------------------------------------
            // FOOD ITEM AND ORDER ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // One FoodItem can appear in many OrderItems.
            // OrderItem.FoodItemId is the foreign key.
            modelBuilder.Entity<OrderItem>()
                .HasOne(orderItem => orderItem.FoodItem)
                .WithMany()
                .HasForeignKey(orderItem => orderItem.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------------------------------
            // FOOD CATEGORY AND FOOD ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // One FoodCategory can contain many FoodItems.
            // FoodItem.FoodCategoryId is the foreign key.
            modelBuilder.Entity<FoodItem>()
                .HasOne(foodItem => foodItem.FoodCategory)
                .WithMany()
                .HasForeignKey(foodItem => foodItem.FoodCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------------------------------
            // APPLICATION USER AND CART RELATIONSHIP
            // ---------------------------------------------------------

            // One registered user can have only one shopping cart.
            // Cart.UserId is the foreign key connected to AspNetUsers.Id.
            //
            // Deleting a user also deletes that user's cart.
            modelBuilder.Entity<Cart>()
                .HasOne(cart => cart.User)
                .WithOne(user => user.Cart)
                .HasForeignKey<Cart>(cart => cart.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Creates a unique database index on UserId.
            // This provides an additional database-level guarantee
            // that one user cannot have multiple carts.
            modelBuilder.Entity<Cart>()
                .HasIndex(cart => cart.UserId)
                .IsUnique();

            // ---------------------------------------------------------
            // CART AND CART ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // One Cart can contain many CartItems.
            // CartItem.CartId is the foreign key.
            //
            // Deleting or clearing a cart record also deletes
            // all CartItems belonging to it.
            modelBuilder.Entity<CartItem>()
                .HasOne(cartItem => cartItem.Cart)
                .WithMany(cart => cart.CartItems)
                .HasForeignKey(cartItem => cartItem.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------------------------------------------------
            // FOOD ITEM AND CART ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // One FoodItem can appear in multiple customer carts.
            // CartItem.FoodItemId is the foreign key.
            //
            // Restrict prevents deletion of a FoodItem while it is
            // still referenced by any customer's cart.
            modelBuilder.Entity<CartItem>()
                .HasOne(cartItem => cartItem.FoodItem)
                .WithMany()
                .HasForeignKey(cartItem => cartItem.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevents the same FoodItem from being stored as multiple
            // CartItem rows inside the same cart.
            //
            // When the same food item is added again, its Quantity
            // will be increased by the service instead.
            modelBuilder.Entity<CartItem>()
                .HasIndex(cartItem => new
                {
                    cartItem.CartId,
                    cartItem.FoodItemId
                })
                .IsUnique();

            // ---------------------------------------------------------
            // DECIMAL COLUMN CONFIGURATION
            // ---------------------------------------------------------

            // Stores OrderItem.UnitPrice with two decimal places.
            modelBuilder.Entity<OrderItem>()
                .Property(orderItem => orderItem.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // Stores Order.TotalAmount with two decimal places.
            modelBuilder.Entity<Order>()
                .Property(order => order.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Stores FoodItem.Price with two decimal places.
            modelBuilder.Entity<FoodItem>()
                .Property(foodItem => foodItem.Price)
                .HasColumnType("decimal(18,2)");

            // ---------------------------------------------------------
            // FOOD CATEGORY SEED DATA
            // ---------------------------------------------------------

            modelBuilder.Entity<FoodCategory>().HasData(
                new FoodCategory
                {
                    Id = 1,
                    CategoryName = "Pizza",
                    IsActive = true
                },
                new FoodCategory
                {
                    Id = 2,
                    CategoryName = "Burger",
                    IsActive = true
                },
                new FoodCategory
                {
                    Id = 3,
                    CategoryName = "Biryani",
                    IsActive = true
                },
                new FoodCategory
                {
                    Id = 4,
                    CategoryName = "Drinks",
                    IsActive = true
                },
                new FoodCategory
                {
                    Id = 5,
                    CategoryName = "Desserts",
                    IsActive = true
                }
            );
        }
    }
}