using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text.Json;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Enums;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.TaskManagementAPI.Tests;

public class ServiceBusHandlerTests
{
    private readonly Mock<ServiceBusClient> _mockClient;
    private readonly Mock<ServiceBusSender> _mockSender;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<ISiteTaskService> _mockSiteTaskService;
    private readonly ServiceBusHandler _serviceBusHandler;

    public ServiceBusHandlerTests()
    {
        _mockClient = new Mock<ServiceBusClient>();
        _mockSender = new Mock<ServiceBusSender>();
        _mockConfig = new Mock<IConfiguration>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockSiteTaskService = new Mock<ISiteTaskService>();

        // Mock configuration setup
        _mockConfig.SetupGet(x => x["ServiceBus:ConnectionString"]).Returns("fake-connection-string");
        _mockConfig.SetupGet(x => x["ServiceBus:QueueName"]).Returns("tasks-queue");
        _mockConfig.SetupGet(x => x["ServiceBus:CompletionEventQueueName"]).Returns("completion-events-queue");
        _mockConfig.SetupGet(x => x["ServiceBus:ReceivingMaxWaitTimeInSeconds"]).Returns("5");

        // Setup the mock client to return a mock sender
        _mockClient.Setup(x => x.CreateSender(It.IsAny<string>())).Returns(_mockSender.Object);

        // Configure IServiceScopeFactory to return a mock IServiceScope
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);

        // Configure IServiceScope to resolve ISiteTaskService
        _mockScope.Setup(x => x.ServiceProvider.GetService(typeof(ISiteTaskService)))
            .Returns(_mockSiteTaskService.Object);

        // Initialize ServiceBusHandler with mock dependencies
        _serviceBusHandler = new ServiceBusHandler(
            _mockConfig.Object,
            _mockScopeFactory.Object,
            _mockClient.Object);
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

        _mockSiteTaskService.Setup(x => x.UpdateSiteTaskStatus(It.IsAny<UpdateSiteTaskStatusDTO>()))
            .ReturnsAsync(true);

        // Act
        await _serviceBusHandler.SendMessageAsync(siteTask);

        // Assert
        _mockSender.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
        _mockSiteTaskService.Verify(x => x.UpdateSiteTaskStatus(It.Is<UpdateSiteTaskStatusDTO>(dto => dto.Id == siteTask.Id && dto.Status == SiteTaskStatus.InProgress)), Times.Once);

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

        _mockSiteTaskService.Setup(x => x.UpdateSiteTaskStatus(It.IsAny<UpdateSiteTaskStatusDTO>()))
            .ReturnsAsync(true);

        // Initialize ServiceBusHandler with mock dependencies
        var serviceBusHandler = new ServiceBusHandler(
            _mockConfig.Object,
            _mockScopeFactory.Object,
            _mockClient.Object,
            mockReceiver.Object);

        // Act
        var tasks = await serviceBusHandler.ReceiveMessagesAsync(5);

        // Assert
        tasks.Should().HaveCount(1);
        tasks[0].Id.Should().Be(1);
        tasks[0].Name.Should().Be("Test Task");
        tasks[0].Status.Should().Be(SiteTaskStatus.InProgress);

        mockReceiver.Verify(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), default), Times.Once);
        _mockSiteTaskService.Verify(x => x.UpdateSiteTaskStatus(It.Is<UpdateSiteTaskStatusDTO>(dto => dto.Id == 1 && dto.Status == SiteTaskStatus.Completed)), Times.Once);
    }
}
