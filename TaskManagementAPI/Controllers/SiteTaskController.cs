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
        [FromBody] SiteTask siteTask,
        [FromServices] ServiceBusHandler serviceBusHandler)
    {
        await _siteTaskService.AddSiteTask(siteTask);
        await serviceBusHandler.SendMessageAsync(siteTask);
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
}
