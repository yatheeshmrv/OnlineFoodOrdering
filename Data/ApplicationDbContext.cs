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

        // Represents reusable delivery addresses saved by customers.
        public DbSet<UserAddress> UserAddresses { get; set; }

        // Configures relationships, columns, indexes and seed data.
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            // Applies the default ASP.NET Core Identity configuration.
            // This must be called before adding custom configurations.
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
            // APPLICATION USER AND SAVED ADDRESS RELATIONSHIP
            // ---------------------------------------------------------

            // One registered customer can save many delivery addresses.
            // UserAddress.UserId is connected to AspNetUsers.Id.
            //
            // Deleting the user also deletes all saved addresses.
            // Historical Order address snapshots remain unchanged.
            modelBuilder.Entity<UserAddress>()
                .HasOne(address => address.User)
                .WithMany(user => user.UserAddresses)
                .HasForeignKey(address => address.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Speeds up queries that retrieve all addresses belonging
            // to the currently authenticated customer.
            modelBuilder.Entity<UserAddress>()
                .HasIndex(address => address.UserId)
                .HasDatabaseName("IX_UserAddresses_UserId");

            // Allows only one address with IsDefault = true per user.
            //
            // Multiple non-default addresses are still allowed because
            // this unique index includes only rows where IsDefault is true.
            modelBuilder.Entity<UserAddress>()
                .HasIndex(address => new
                {
                    address.UserId,
                    address.IsDefault
                })
                .IsUnique()
                .HasFilter("[IsDefault] = 1")
                .HasDatabaseName(
                    "UX_UserAddresses_UserId_Default");

            // ---------------------------------------------------------
            // SAVED ADDRESS COLUMN CONFIGURATION
            // ---------------------------------------------------------

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.UserId)
                .IsRequired()
                .HasMaxLength(450);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.AddressLabel)
                .IsRequired()
                .HasMaxLength(30);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.RecipientName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.RecipientPhone)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.AddressLine1)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.AddressLine2)
                .HasMaxLength(200);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.Landmark)
                .HasMaxLength(150);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.City)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.State)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<UserAddress>()
                .Property(address => address.PostalCode)
                .IsRequired()
                .HasMaxLength(10);

            // ---------------------------------------------------------
            // ORDER DELIVERY-ADDRESS SNAPSHOT CONFIGURATION
            // ---------------------------------------------------------

            // These fields remain nullable so existing historical
            // orders can be retained after the migration.
            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryRecipientName)
                .HasMaxLength(100);

            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryPhone)
                .HasMaxLength(20);

            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryAddressLine1)
                .HasMaxLength(200);

            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryAddressLine2)
                .HasMaxLength(200);

            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryLandmark)
                .HasMaxLength(150);

            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryCity)
                .HasMaxLength(100);

            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryState)
                .HasMaxLength(100);

            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryPostalCode)
                .HasMaxLength(10);

            modelBuilder.Entity<Order>()
                .Property(order => order.DeliveryInstructions)
                .HasMaxLength(500);

            // ---------------------------------------------------------
            // ORDER PAYMENT CONFIGURATION
            // ---------------------------------------------------------

            modelBuilder.Entity<Order>()
                .Property(order => order.PaymentMethod)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue(PaymentMethods.CashOnDelivery);

            modelBuilder.Entity<Order>()
                .Property(order => order.PaymentStatus)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue(PaymentStatuses.Pending);

            // ---------------------------------------------------------
            // ORDER AND ORDER ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // One Order can contain many OrderItems.
            // Deleting an order also deletes its order items.
            modelBuilder.Entity<OrderItem>()
                .HasOne(orderItem => orderItem.Order)
                .WithMany(order => order.OrderItems)
                .HasForeignKey(orderItem => orderItem.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------------------------------------------------
            // FOOD ITEM AND ORDER ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // Restrict prevents deleting a FoodItem that is referenced
            // by an existing historical OrderItem.
            modelBuilder.Entity<OrderItem>()
                .HasOne(orderItem => orderItem.FoodItem)
                .WithMany()
                .HasForeignKey(orderItem => orderItem.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------------------------------
            // FOOD CATEGORY AND FOOD ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // One FoodCategory can contain many FoodItems.
            modelBuilder.Entity<FoodItem>()
                .HasOne(foodItem => foodItem.FoodCategory)
                .WithMany()
                .HasForeignKey(foodItem => foodItem.FoodCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------------------------------
            // APPLICATION USER AND CART RELATIONSHIP
            // ---------------------------------------------------------

            // One registered user can have only one shopping cart.
            modelBuilder.Entity<Cart>()
                .HasOne(cart => cart.User)
                .WithOne(user => user.Cart)
                .HasForeignKey<Cart>(cart => cart.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Database-level guarantee that a user cannot have
            // multiple shopping carts.
            modelBuilder.Entity<Cart>()
                .HasIndex(cart => cart.UserId)
                .IsUnique();

            // ---------------------------------------------------------
            // CART AND CART ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // One Cart can contain many CartItems.
            // Deleting a cart also deletes its CartItems.
            modelBuilder.Entity<CartItem>()
                .HasOne(cartItem => cartItem.Cart)
                .WithMany(cart => cart.CartItems)
                .HasForeignKey(cartItem => cartItem.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------------------------------------------------
            // FOOD ITEM AND CART ITEM RELATIONSHIP
            // ---------------------------------------------------------

            // Prevents deleting a FoodItem while it is still referenced
            // by any customer's shopping cart.
            modelBuilder.Entity<CartItem>()
                .HasOne(cartItem => cartItem.FoodItem)
                .WithMany()
                .HasForeignKey(cartItem => cartItem.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevents duplicate rows for the same food item inside
            // one shopping cart.
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

            modelBuilder.Entity<OrderItem>()
                .Property(orderItem => orderItem.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(order => order.TotalAmount)
                .HasColumnType("decimal(18,2)");

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