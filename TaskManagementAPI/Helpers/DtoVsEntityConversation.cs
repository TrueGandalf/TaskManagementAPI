using TaskManagementAPI.Data.Entities;
using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Helpers;

// to implement this test app faster
public static class DtoVsEntityConversation
{
    public static SiteTask ToEntity(this SiteTaskRequestDTO dto)
    {
        return new SiteTask
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status,
            AssignedTo = dto.AssignedTo,
        };
    }

    public static SiteTaskDTO ToDTO(this SiteTask entity)
    {
        return new SiteTaskDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Status = entity.Status,
            AssignedTo = entity.AssignedTo,
        };
    }

    public static SiteTask ToEntity(this SiteTaskDTO dto)
    {
        return new SiteTask
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status,
            AssignedTo = dto.AssignedTo,
        };
    }
}
