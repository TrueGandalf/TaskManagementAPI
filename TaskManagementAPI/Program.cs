using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

        // Register Swagger services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Task Management API",
                Description = "An API to manage tasks and demonstrate service bus integration"
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            // Enable Swagger middleware to serve the generated JSON and UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
                c.RoutePrefix = string.Empty; // Set Swagger UI to be the default route
            });
        }


        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        // Start receiving messages before running the app
        // turned it off to use another implementation
        //await ReceiveMessagesInBackground(app);

        app.Run();
    }

    private static async Task ReceiveMessagesInBackground(WebApplication app)
    {
        var serviceBusHandler = app.Services.GetRequiredService<ServiceBusHandler>();

        async Task ProcessTask(SiteTaskRequestDTO task)
        {
            Console.WriteLine($"Processing task: {task.Name}");
        }

        await serviceBusHandler.StartReceivingMessagesAsync(ProcessTask);
    }

    // to deploy this test project with ease
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
