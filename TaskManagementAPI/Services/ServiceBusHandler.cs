﻿using Azure.Messaging.ServiceBus;
using System.Text.Json;
using TaskManagementAPI.DTOs;
using Polly;
using Polly.Retry;

namespace TaskManagementAPI.Services;

public class ServiceBusHandler
{
    private readonly string _connectionString;
    private readonly string _tasksQueueName;
    private readonly string _completionEventsQueueName;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly TimeSpan _maxWaitTime;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ServiceBusHandler(IConfiguration configuration)
    {
        _connectionString = configuration["ServiceBus:ConnectionString"]!;
        _tasksQueueName = configuration["ServiceBus:QueueName"]!;
        _completionEventsQueueName = configuration["ServiceBus:CompletionEventQueueName"]!;

        _client = new ServiceBusClient(_connectionString);
        _sender = _client.CreateSender(_tasksQueueName);
        
        int maxWaitTimeInSeconds = int.TryParse(
            configuration["ServiceBus:ReceivingMaxWaitTimeInSeconds"], out var result)
            ? result : 5;

        _maxWaitTime = TimeSpan.FromSeconds(maxWaitTimeInSeconds);

        _retryPolicy = Policy
            .Handle<Exception>(ex => IsTransient(ex))
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} for Service Bus operation due to error: {exception.Message}");
                });
    }

    public async Task SendMessageAsync(SiteTaskDTO siteTask)
    {
        var messageBody = JsonSerializer.Serialize(siteTask);
        var message = new ServiceBusMessage(messageBody);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                await _sender.SendMessageAsync(message);
                // this is a test app and it can be freezed sometimes
                // cause we don't use async logging
                _ = Task.Run(() => Console.WriteLine($"Message sent: {messageBody}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                throw;
            }
        });
    }

    public async Task StartReceivingMessagesAsync(Func<SiteTaskDTO, Task> processTask)
    {
        ServiceBusProcessor processor = _client.CreateProcessor(
            _tasksQueueName, new ServiceBusProcessorOptions()); // remove , new ServiceBusProcessorOptions()

        processor.ProcessMessageAsync += async args =>
        {
            var messageBody = args.Message.Body.ToString();
            var siteTask = JsonSerializer.Deserialize<SiteTaskDTO>(messageBody);

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

    public async Task<List<SiteTaskDTO>> ReceiveMessagesAsync(int maxMessagesCount)
    {
        var tasks = new List<SiteTaskDTO>();

        // Create the receiver outside the retry logic to avoid recreating it on every retry
        var receiver = _client.CreateReceiver(_tasksQueueName);

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var receivedMessages = await receiver.ReceiveMessagesAsync(
                    maxMessages: maxMessagesCount,
                    maxWaitTime: _maxWaitTime);

                foreach (var message in receivedMessages)
                {
                    var messageBody = message.Body.ToString();
                    var siteTask = JsonSerializer.Deserialize<SiteTaskDTO>(messageBody);

                    if (siteTask != null)
                    {
                        tasks.Add(siteTask);
                        await receiver.CompleteMessageAsync(message);

                        // Create and send the completion event for each processed task
                        var completionEvent = new SiteTaskCompletionEvent
                        {
                            TaskId = siteTask.Id,
                            TaskName = siteTask.Name,
                            Status = siteTask.Status.ToString(),
                            CompletedAt = DateTime.UtcNow
                        };
                        await SendCompletionEventAsync(completionEvent);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving messages: {ex.Message}");
            throw;
        }
        finally
        {
            await receiver.DisposeAsync();
        }

        return tasks;
    }

    public async Task<List<SiteTaskCompletionEvent>> ReceiveCompletionEventsAsync(int maxMessagesCount)
    {
        var events = new List<SiteTaskCompletionEvent>();

        var receiver = _client.CreateReceiver(_completionEventsQueueName);

        try
        {
            var receivedMessages = await receiver.ReceiveMessagesAsync(
                maxMessages: maxMessagesCount,
                maxWaitTime: _maxWaitTime);

            foreach (var message in receivedMessages)
            {
                var messageBody = message.Body.ToString();
                var completionEvent = JsonSerializer.Deserialize<SiteTaskCompletionEvent>(messageBody);

                if (completionEvent != null)
                {
                    events.Add(completionEvent);
                    await receiver.CompleteMessageAsync(message);
                }
            }
        }
        finally
        {
            await receiver.DisposeAsync();
        }

        return events;
    }

    public async Task SendCompletionEventAsync(SiteTaskCompletionEvent completionEvent)
    {
        var sender = _client.CreateSender(_completionEventsQueueName);
        var messageBody = JsonSerializer.Serialize(completionEvent);
        ServiceBusMessage message = new ServiceBusMessage(messageBody);

        try
        {
            await sender.SendMessageAsync(message);
            Console.WriteLine($"Completion event sent: {messageBody}");
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    private bool IsTransient(Exception ex)
    {
        // Logic to determine if the exception is transient (network errors, timeouts, etc.)
        return ex is ServiceBusException sbEx && sbEx.Reason == ServiceBusFailureReason.ServiceTimeout;
    }
}
