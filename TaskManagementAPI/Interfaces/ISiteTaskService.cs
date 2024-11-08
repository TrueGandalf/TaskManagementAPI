using TaskManagementAPI.DTOs;
using TaskManagementAPI.Enums;

namespace TaskManagementAPI.Interfaces;

public interface ISiteTaskService
{
    Task<SiteTaskDTO> AddSiteTask(SiteTaskDTO siteTask);
    Task DeleteSiteTask(int taskId);
    Task UpdateSiteTaskStatus(int taskId, SiteTaskStatus newStatus);
    Task<IEnumerable<SiteTaskDTO>> GetAllSiteTasks();
}
