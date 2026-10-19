using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Context;

public class PetroVisionDbContext : DbContext
{
    public PetroVisionDbContext(DbContextOptions<PetroVisionDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQL Server returns DateTime with Kind=Unspecified; mark everything as UTC.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    private class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
    }
}
