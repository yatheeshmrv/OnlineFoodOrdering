using FoodOrderAPI.Data;
using Microsoft.EntityFrameworkCore;
using FoodOrderAPI.Repositories;
using FoodOrderAPI.Services;
using FoodOrderAPI.ExceptionHandlers;

namespace OnlineFoodOrdering
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

            builder.Services.AddScoped<IFoodItemService, FoodItemService>();

            builder.Services.AddScoped<IFoodItemRepository, FoodItemRepository>();

            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            
            builder.Services.AddScoped<IOrderService, OrderService>();

            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //Registering Dbcontext in program.cs

            builder.Services.AddScoped<IFoodCategoryRepository, FoodCategoryRepository>();
            builder.Services.AddScoped<IFoodCategoryService, FoodCategoryService>();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            app.UseExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
