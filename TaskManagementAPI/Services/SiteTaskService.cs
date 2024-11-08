using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Helpers;
using TaskManagementAPI.Interfaces;

namespace TaskManagementAPI.Services;

public class SiteTaskService : ISiteTaskService
{
    private readonly ApplicationDbContext _context;

    public SiteTaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SiteTaskDTO> AddSiteTask(SiteTaskRequestDTO siteTask)
    {
        var entity = siteTask.ToEntity();
        var result = _context.SiteTasks.Add(entity);
        await _context.SaveChangesAsync();
        return entity.ToDTO();
    }

    public async Task<bool> UpdateSiteTaskStatus(UpdateSiteTaskStatusDTO siteTaskWithNewStatus)
    {
        var siteTask = await _context.SiteTasks.FindAsync(siteTaskWithNewStatus.Id);

        if (siteTask != null)
        {
            siteTask.Status = siteTaskWithNewStatus.Status;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<IEnumerable<SiteTaskDTO>> GetAllSiteTasks()
    {
        return (await _context.SiteTasks.ToListAsync()).Select(x => x.ToDTO()).ToList();
    }

    public async Task DeleteSiteTask(int id)
    {
        var siteTask = await _context.SiteTasks.FindAsync(id);

        if (siteTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {id} was not found.");
        }

        _context.SiteTasks.Remove(siteTask);
        await _context.SaveChangesAsync();
    }

    public async Task<SiteTaskDTO> GetSiteTask(int id)
    {
        var siteTask = await _context.SiteTasks.FindAsync(id);

        if (siteTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {id} was not found.");
        }

        return siteTask.ToDTO();
    }
}
