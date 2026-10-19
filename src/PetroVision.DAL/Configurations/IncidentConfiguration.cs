using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> b)
    {
        b.Property(x => x.EvidenceUrl).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(1000);

        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CreatedAt);

        b.HasOne(x => x.Camera).WithMany()
         .HasForeignKey(x => x.CameraId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Handler).WithMany()
         .HasForeignKey(x => x.HandlerId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
