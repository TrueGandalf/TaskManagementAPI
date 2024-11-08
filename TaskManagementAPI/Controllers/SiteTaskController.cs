using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Enums;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Services;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SiteTaskController : ControllerBase
{
    private readonly ISiteTaskService _siteTaskService;

    public SiteTaskController(ISiteTaskService siteTaskService)
    {
        _siteTaskService = siteTaskService;
    }

    // todo: maybe delete excess endpoint names
    [HttpPost("add")]
    public async Task<IActionResult> AddSiteTask(
        [FromBody] SiteTaskDTO siteTask,
        [FromServices] ServiceBusHandler serviceBusHandler)
    {
        var result = await _siteTaskService.AddSiteTask(siteTask);
        await serviceBusHandler.SendMessageAsync(result);
        return Ok("Task added and message sent to Service Bus.");
    }

    // todo: change put request so it will use a json request with id and new SiteTaskStatus
    [HttpPut("update-status/{id}")]
    public async Task<IActionResult> UpdateSiteTaskStatus(int id, [FromBody] SiteTaskStatus newStatus)
    {
        await _siteTaskService.UpdateSiteTaskStatus(id, newStatus);
        return Ok("Task status updated successfully.");
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllSiteTasks()
    {
        var siteTasks = await _siteTaskService.GetAllSiteTasks();
        return Ok(siteTasks);
    }

    [HttpGet("receive-messages")]
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

    [HttpGet("receive-completion-events")]
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
