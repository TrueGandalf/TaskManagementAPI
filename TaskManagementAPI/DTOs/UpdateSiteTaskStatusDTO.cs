using TaskManagementAPI.Enums;

namespace TaskManagementAPI.DTOs;

public class UpdateSiteTaskStatusDTO
{
    public int Id { get; set; }
    public SiteTaskStatus Status { get; set; }
}
