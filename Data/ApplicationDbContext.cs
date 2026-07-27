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