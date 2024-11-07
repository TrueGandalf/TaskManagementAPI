namespace TaskManagementAPI.DTOs;

public class SiteTaskCompletionEvent
{
    public int TaskId { get; set; }
    public string TaskName { get; set; }
    public string Status { get; set; }
    public DateTime CompletedAt { get; set; }
}
