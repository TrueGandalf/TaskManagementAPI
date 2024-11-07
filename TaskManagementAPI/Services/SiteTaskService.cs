using TaskManagementAPI.DTOs;
using TaskManagementAPI.Enums;
using TaskManagementAPI.Interfaces;

namespace TaskManagementAPI.Services;

public class SiteTaskService : ISiteTaskService
{
    private readonly List<SiteTask> _siteTasks = new(); // InMemory test approach temp instead of EF

    public async Task AddSiteTask(SiteTask siteTask)
    {
        siteTask.Id = (_siteTasks.LastOrDefault()?.Id ?? 0) + 1;
        _siteTasks.Add(siteTask);
    }

    public async Task UpdateSiteTaskStatus(int id, SiteTaskStatus newStatus)
    {
        var siteTask = _siteTasks.SingleOrDefault(t => t.Id == id);

        if (siteTask == null)
        {
            // todo: inform front-end that there is no such task
            return;
        }

        siteTask!.Status = newStatus;
    }

    public async Task<IEnumerable<SiteTask>> GetAllSiteTasks()
    {
        return _siteTasks;
    }

    public async Task DeleteSiteTask(int id)
    {
        // todo: maybe remove this repeat
        var siteTask = _siteTasks.SingleOrDefault(t => t.Id == id);

        if (siteTask == null)
        {
            // todo: inform front-end that there is no such task
            return;
        }

        _siteTasks.Remove(siteTask!);
    }
}
