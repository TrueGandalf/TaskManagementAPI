using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Services;

namespace TaskManagementAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        SubstituteConfigurations(builder);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagementDB")));

        // Add services to the container.
        builder.Services.AddScoped<ISiteTaskService, SiteTaskService>();
        builder.Services.AddSingleton<ServiceBusHandler>();

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

        // Start receiving messages before running the app
        // turned it of to use another implementation
        //await ReceiveMessagesInBackground(app);

        app.Run();
    }

    private static async Task ReceiveMessagesInBackground(WebApplication app)
    {
        var serviceBusHandler = app.Services.GetRequiredService<ServiceBusHandler>();

        async Task ProcessTask(SiteTaskDTO task)
        {
            Console.WriteLine($"Processing task: {task.Name}");
            // todo: add some logic here
        }

        await serviceBusHandler.StartReceivingMessagesAsync(ProcessTask);
    }

    private static void SubstituteConfigurations(WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var serviceBusConnectionString =
            Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTION_STRING")
            ?? configuration["ServiceBus:ConnectionString"];

        var taskManagementDbConnectionString =
            Environment.GetEnvironmentVariable("TASK-MANAGEMENT-DB-CONNECTION-STRING")
            ?? configuration["ConnectionStrings:TaskManagementDB"];

        configuration["ServiceBus:ConnectionString"] = serviceBusConnectionString;
        configuration["ConnectionStrings:TaskManagementDB"] = taskManagementDbConnectionString;
    }
}
