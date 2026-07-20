using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Data
{
    // ApplicationDbContext is the main database context class.
    // It connects our C# models with the database tables.
    public class ApplicationDbContext : DbContext
    {
        // Constructor receives DbContextOptions from Program.cs.
        // This is how SQL Server connection is passed to this context.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // This is where we can configure the database provider and connection string.
            // For example, we can use SQL Server with a connection string.
            // Uncomment the line below to use SQL Server with a local database.
             optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FoodOrderDB;Trusted_Connection=True;");
        }

        // DbSet represents the FoodCategories table in the database.
        public DbSet<FoodCategory> FoodCategories { get; set; }

        // DbSet represents the FoodItems table in the database.
        public DbSet<FoodItem> FoodItems { get; set; }

        // DbSet represents the Orders table in the database.
        public DbSet<Order> Orders { get; set; }

        // DbSet represents the OrderItems table in the database.
        public DbSet<OrderItem> OrderItems { get; set; }

        // OnModelCreating is used to configure table relationships and column rules.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Calls the base EF Core configuration first.
            base.OnModelCreating(modelBuilder);

            // Relationship:
            // One Order can have many OrderItems.
            // OrderItem.OrderId is the foreign key.
            modelBuilder.Entity<OrderItem>()
                .HasOne(orderItem => orderItem.Order)
                .WithMany(order => order.OrderItems)
                .HasForeignKey(orderItem => orderItem.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship:
            // One FoodItem can appear in many OrderItems.
            // OrderItem.FoodItemId is the foreign key.
            modelBuilder.Entity<OrderItem>()
                .HasOne(orderItem => orderItem.FoodItem)
                .WithMany()
                .HasForeignKey(orderItem => orderItem.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // UnitPrice column should store decimal values safely.
            // Example: 199.99
            modelBuilder.Entity<OrderItem>()
                .Property(orderItem => orderItem.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // TotalAmount column should also store decimal values safely.
            // Example: 499.50
            modelBuilder.Entity<Order>()
                .Property(order => order.TotalAmount)
                .HasColumnType("decimal(18,2)");
        }
    }
}