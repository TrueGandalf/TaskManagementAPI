using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.Controllers;

/// <summary>
/// Controller for managing site tasks and handling Service Bus communication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SiteTaskController : ControllerBase
{
    private readonly ISiteTaskService _siteTaskService;

    public SiteTaskController(ISiteTaskService siteTaskService)
    {
        _siteTaskService = siteTaskService;
    }

    /// <summary>
    /// Adds a new task and sends a message to the Service Bus.
    /// </summary>
    /// <param name="siteTask">The task details to add.</param>
    /// <param name="serviceBusHandler">The Service Bus handler used to send a message.</param>
    /// <returns>A success message confirming task addition and message sending.</returns>
    [HttpPost]
    public async Task<IActionResult> AddSiteTask(
        [FromBody] SiteTaskDTO siteTask,
        [FromServices] ServiceBusHandler serviceBusHandler)
    {
        var result = await _siteTaskService.AddSiteTask(siteTask);
        await serviceBusHandler.SendMessageAsync(result);
        return Ok("Task added and message sent to Service Bus.");
    }

    /// <summary>
    /// Updates the status of an existing task.
    /// </summary>
    /// <param name="siteTaskWithNewStatus">The task ID and new status.</param>
    /// <returns>
    /// A success message if the task status was updated, or a not found response if the task with the specified ID doesn’t exist.
    /// </returns>
    [HttpPut]
    public async Task<IActionResult> UpdateSiteTaskStatus(
        [FromBody] UpdateSiteTaskStatusDTO siteTaskWithNewStatus)
    {
        bool isUpdated = await _siteTaskService.UpdateSiteTaskStatus(siteTaskWithNewStatus);

        if (!isUpdated)
        {
            return NotFound($"Task with ID {siteTaskWithNewStatus.Id} was not found.");
        }

        return Ok("Task status updated successfully.");
    }

    /// <summary>
    /// Retrieves all tasks.
    /// </summary>
    /// <returns>A list of all site tasks.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllSiteTasks()
    {
        var siteTasks = await _siteTaskService.GetAllSiteTasks();
        return Ok(siteTasks);
    }

    /// <summary>
    /// Retrieves a specific task by its ID.
    /// </summary>
    /// <param name="id">The ID of the task to retrieve.</param>
    /// <returns>The task details, or a not found result if the task doesn't exist.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSiteTask([FromRoute] int id)
    {
        var siteTasks = await _siteTaskService.GetSiteTask(id);
        return Ok(siteTasks);
    }

    /// <summary>
    /// Receives and processes a specified number of messages from the Service Bus.
    /// </summary>
    /// <param name="count">The maximum number of messages to retrieve.</param>
    /// <param name="serviceBusHandler">The Service Bus handler used to receive messages.</param>
    /// <returns>A list of tasks from received messages, or a message if no messages are found.</returns>
    [HttpGet("messages")]
    public async Task<IActionResult> ReceiveMessages(
        [FromQuery] int count,
        [FromServices] ServiceBusHandler serviceBusHandler)
    {
        var tasks = await serviceBusHandler.ReceiveMessagesAsync(count);

        if (tasks.Count == 0)
        {
            return Ok("No messages found in the queue");
        }

        return Ok(tasks);
    }

    /// <summary>
    /// Receives and processes a specified number of completion events from the Service Bus.
    /// </summary>
    /// <param name="count">The maximum number of events to retrieve.</param>
    /// <param name="serviceBusHandler">The Service Bus handler used to receive events.</param>
    /// <returns>A list of completion events, or a message if no events are found.</returns>
    [HttpGet("events")]
    public async Task<IActionResult> ReceiveCompletionEvents(
        [FromQuery] int count,
        [FromServices] ServiceBusHandler serviceBusHandler)
    {
        var events = await serviceBusHandler.ReceiveCompletionEventsAsync(count);

        if (events.Count == 0)
        {
            return Ok("No events found in the queue");
        }

        return Ok(events);
    }
}
