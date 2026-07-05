using FoodOrderAPI.Repositories;
using FoodOrderAPI.Services;
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

            var app = builder.Build();

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
