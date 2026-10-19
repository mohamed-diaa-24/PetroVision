using Microsoft.EntityFrameworkCore;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Context;

public class PetroVisionDbContext : DbContext
{
    public PetroVisionDbContext(DbContextOptions<PetroVisionDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<AgentPermission> AgentPermissions => Set<AgentPermission>();
    public DbSet<Camera> Cameras => Set<Camera>();
    public DbSet<CameraModelConfig> CameraModelConfigs => Set<CameraModelConfig>();
    public DbSet<CameraRequest> CameraRequests => Set<CameraRequest>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Incident> Incidents => Set<Incident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetroVisionDbContext).Assembly);
    }
}
