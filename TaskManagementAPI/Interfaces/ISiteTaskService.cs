using TaskManagementAPI.DTOs;
using TaskManagementAPI.Enums;

namespace TaskManagementAPI.Interfaces;

public interface ISiteTaskService
{
    Task AddSiteTask(SiteTask siteTask);
    Task DeleteSiteTask(int taskId);
    Task UpdateSiteTaskStatus(int taskId, SiteTaskStatus newStatus);
    Task<IEnumerable<SiteTask>> GetAllSiteTasks();
}
