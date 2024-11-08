using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Enums;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.TaskManagementAPI.Tests;

public class SiteTaskServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly ISiteTaskService _siteTaskService;

    public SiteTaskServiceTests()
    {
        // Configure in-memory database options
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        // Create an instance of ApplicationDbContext with in-memory database
        _context = new ApplicationDbContext(options);

        // Initialize the service with the in-memory context
        _siteTaskService = new SiteTaskService(_context);
    }

    [Fact]
    public async Task AddTaskAsync_ShouldAddTask()
    {
        // Arrange
        var siteTask = new SiteTaskRequestDTO
        {
            Id = 1, // must be ignored after deserialization
            Name = "New Task",
            Status = SiteTaskStatus.Completed, // must be ignored after deserialization
        };

        var asJson = JsonSerializer.Serialize(siteTask);
        var asObject = JsonSerializer.Deserialize<SiteTaskRequestDTO>(asJson);

        // Act
        await _siteTaskService.AddSiteTask(asObject);
        var tasks = await _context.SiteTasks.ToListAsync();

        // Assert
        tasks.Should().HaveCount(1);
        tasks[0].Name.Should().Be("New Task");
        tasks[0].Status.Should().Be(SiteTaskStatus.NotStarted);
    }
}
