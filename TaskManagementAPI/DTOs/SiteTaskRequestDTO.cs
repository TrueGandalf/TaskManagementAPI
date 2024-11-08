using System.Text.Json.Serialization;
using TaskManagementAPI.Enums;

namespace TaskManagementAPI.DTOs;

public class SiteTaskRequestDTO
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Name { get; set; }

    public string? Description { get; set; }

    [JsonIgnore]
    public SiteTaskStatus Status { get; set; }
    public string? AssignedTo { get; set; }
}
