using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data.Entities;

namespace TaskManagementAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<SiteTask> SiteTasks { get; set; }
}
