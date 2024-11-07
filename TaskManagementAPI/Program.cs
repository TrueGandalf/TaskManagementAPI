using System.Text.Json.Serialization;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Services;

namespace TaskManagementAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.            

            builder.Services.AddSingleton<ISiteTaskService, SiteTaskService>(); // singleton for temp inmemory testing

            // Configure controllers with JSON options for case-insensitive enum handling
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Allows case-insensitive property names
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Converts enums to/from strings
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
