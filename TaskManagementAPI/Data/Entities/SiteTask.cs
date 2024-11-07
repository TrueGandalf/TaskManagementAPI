using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TaskManagementAPI.Enums;

namespace TaskManagementAPI.Data.Entities;

public class SiteTask
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public SiteTaskStatus Status { get; set; }
    public string? AssignedTo { get; set; }
}
