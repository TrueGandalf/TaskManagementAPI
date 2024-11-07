using Azure.Messaging.ServiceBus;
using System.Text.Json;
using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services;

public class ServiceBusHandler
{
    private readonly string _connectionString;
    private readonly string _queueName;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusHandler(IConfiguration configuration)
    {
        _connectionString = configuration["ServiceBus:ConnectionString"]!;
        _queueName = configuration["ServiceBus:QueueName"]!;

        _client = new ServiceBusClient(_connectionString);
        _sender = _client.CreateSender(_queueName);
    }

    public async Task SendMessageAsync(SiteTask siteTask)
    {
        var messageBody = JsonSerializer.Serialize(siteTask);
        var message = new ServiceBusMessage(messageBody);

        try
        {
            await _sender.SendMessageAsync(message);
            Console.WriteLine($"Message sent: {messageBody}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            // todo: implement retry logic/log the exception
        }
    }

    public async Task StartReceivingMessagesAsync(Func<SiteTask, Task> processTask)
    {
        ServiceBusProcessor processor = _client.CreateProcessor(
            _queueName, new ServiceBusProcessorOptions()); // remove , new ServiceBusProcessorOptions()

        processor.ProcessMessageAsync += async args =>
        {
            var messageBody = args.Message.Body.ToString();
            var siteTask = JsonSerializer.Deserialize<SiteTask>(messageBody);

            if (siteTask != null)
            {
                await processTask(siteTask);
                Console.WriteLine($"Message received and processed: {messageBody}");
            }

            await args.CompleteMessageAsync(args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine($"Error receiving messeage: {args.Exception.Message}");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
    }
}
