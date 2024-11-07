using TaskManagementAPI.Enums;

namespace TaskManagementAPI.DTOs;

public class SiteTaskDTO
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string? Description { get; set; }
    public SiteTaskStatus Status { get; set; }
    public string? AssignedTo { get; set; }
}
