using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Interfaces;

public interface ISiteTaskService
{
    Task<SiteTaskDTO> AddSiteTask(SiteTaskDTO siteTask);
    Task DeleteSiteTask(int taskId);
    Task<bool> UpdateSiteTaskStatus(UpdateSiteTaskStatusDTO siteTaskWithNewStatus);
    Task<IEnumerable<SiteTaskDTO>> GetAllSiteTasks();
    Task<SiteTaskDTO> GetSiteTask(int id);
}
