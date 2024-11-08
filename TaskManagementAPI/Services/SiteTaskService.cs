using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Enums;
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

    public async Task<SiteTaskDTO> AddSiteTask(SiteTaskDTO siteTask)
    {
        var entity = siteTask.ToEntity();
        var result = _context.SiteTasks.Add(entity);
        await _context.SaveChangesAsync();
        return entity.ToDTO();
    }

    public async Task UpdateSiteTaskStatus(int id, SiteTaskStatus newStatus)
    {
        var siteTask = await _context.SiteTasks.FindAsync(id);

        if (siteTask != null)
        {
            siteTask.Status = newStatus;
            await _context.SaveChangesAsync();
        }
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
}
