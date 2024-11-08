using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using TaskManagementAPI.Data.Entities;
using TaskManagementAPI.Data;
using TaskManagementAPI.Enums;
using TaskManagementAPI.Services;
using Xunit;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Interfaces;
using FluentAssertions;

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
        var siteTask = new SiteTask
        {
            Id = 1,
            Name = "New Task",
            Status = SiteTaskStatus.NotStarted
        };

        // Act
        await _siteTaskService.AddSiteTask(siteTask.ToDTO());
        var tasks = await _context.SiteTasks.ToListAsync();

        // Assert
        tasks.Should().HaveCount(1);
        tasks[0].Name.Should().Be("New Task");
        tasks[0].Status.Should().Be(SiteTaskStatus.NotStarted);
    }
}
