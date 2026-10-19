using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class CameraConfiguration : IEntityTypeConfiguration<Camera>
{
    public void Configure(EntityTypeBuilder<Camera> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(50);
        b.Property(x => x.Location).IsRequired().HasMaxLength(100);
        b.Property(x => x.StreamUrl).HasMaxLength(500);

        b.HasIndex(x => x.Name).IsUnique();
    }
}
