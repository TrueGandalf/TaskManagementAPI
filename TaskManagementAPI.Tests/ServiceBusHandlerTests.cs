using Moq;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Services;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Xunit;
using Azure.Messaging.ServiceBus;
using TaskManagementAPI.Enums;

namespace TaskManagementAPI.TaskManagementAPI.Tests;

public class ServiceBusHandlerTests
{
    private readonly Mock<ServiceBusClient> _mockClient;
    private readonly Mock<ServiceBusSender> _mockSender;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly ServiceBusHandler _serviceBusHandler;

    public ServiceBusHandlerTests()
    {
        _mockClient = new Mock<ServiceBusClient>();
        _mockSender = new Mock<ServiceBusSender>();
        _mockConfig = new Mock<IConfiguration>();

        // Mock configuration setup
        _mockConfig.SetupGet(x => x["ServiceBus:ConnectionString"]).Returns("fake-connection-string");
        _mockConfig.SetupGet(x => x["ServiceBus:QueueName"]).Returns("tasks-queue");
        _mockConfig.SetupGet(x => x["ServiceBus:CompletionEventQueueName"]).Returns("completion-events-queue");
        _mockConfig.SetupGet(x => x["ServiceBus:ReceivingMaxWaitTimeInSeconds"]).Returns("5");

        // Setup the mock client to return a mock sender
        _mockClient.Setup(x => x.CreateSender(It.IsAny<string>())).Returns(_mockSender.Object);

        // Initialize ServiceBusHandler with mock dependencies
        _serviceBusHandler = new ServiceBusHandler(_mockConfig.Object, _mockClient.Object);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSendMessage()
    {
        // Arrange
        var siteTask = new SiteTaskDTO {
            Id = 1,
            Name = "Test Task",
            Status = SiteTaskStatus.NotStarted };
        _mockSender.Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _serviceBusHandler.SendMessageAsync(siteTask);

        // Assert
        _mockSender.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
    }

    [Fact]
    public async Task ReceiveMessagesAsync_ShouldReceiveAndProcessMessages()
    {
        // Arrange
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var messages = new List<ServiceBusReceivedMessage>
        {
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: BinaryData.FromString(JsonSerializer.Serialize(
                    new SiteTaskDTO { Id = 1, Name = "Test Task", Status = SiteTaskStatus.InProgress }))
            )
        };

        mockReceiver.Setup(x => x.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), default))
                    .ReturnsAsync(messages);

        mockReceiver.Setup(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), default))
                    .Returns(Task.CompletedTask);

        // Initialize ServiceBusHandler with mock dependencies
        var _serviceBusHandler2 = new ServiceBusHandler(_mockConfig.Object, _mockClient.Object, mockReceiver.Object);

        // Act
        var tasks = await _serviceBusHandler2.ReceiveMessagesAsync(5);

        // Assert
        tasks.Should().HaveCount(1);
        tasks[0].Id.Should().Be(1);
        tasks[0].Name.Should().Be("Test Task");
        tasks[0].Status.Should().Be(SiteTaskStatus.InProgress);

        mockReceiver.Verify(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), default), Times.Once);
    }
}
