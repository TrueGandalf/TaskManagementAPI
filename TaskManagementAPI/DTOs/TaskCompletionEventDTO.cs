namespace TaskManagementAPI.DTOs;

public class SiteTaskCompletionEventDTO
{
    public int TaskId { get; set; }
    public string TaskName { get; set; }
    public string TaskStatus { get; set; }
    public DateTime TaskCompletedAt { get; set; }
}
