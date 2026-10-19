using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> b)
    {
        b.Property(x => x.Reason).IsRequired().HasMaxLength(250);
        b.Property(x => x.EvidenceUrl).HasMaxLength(500);

        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CreatedAt);

        b.HasOne(x => x.Camera).WithMany()
         .HasForeignKey(x => x.CameraId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.HandledBy).WithMany()
         .HasForeignKey(x => x.HandledById)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
