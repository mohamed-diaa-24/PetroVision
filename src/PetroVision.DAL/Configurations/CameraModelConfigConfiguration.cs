using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class CameraModelConfigConfiguration : IEntityTypeConfiguration<CameraModelConfig>
{
    public void Configure(EntityTypeBuilder<CameraModelConfig> b)
    {
        b.HasIndex(x => new { x.CameraId, x.Type }).IsUnique();

        b.HasOne(x => x.Camera)
         .WithMany(c => c.ModelConfigs)
         .HasForeignKey(x => x.CameraId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
