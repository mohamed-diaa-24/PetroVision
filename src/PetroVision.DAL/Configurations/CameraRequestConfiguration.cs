using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class CameraRequestConfiguration : IEntityTypeConfiguration<CameraRequest>
{
    public void Configure(EntityTypeBuilder<CameraRequest> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(50);
        b.Property(x => x.Location).IsRequired().HasMaxLength(100);
        b.Property(x => x.StreamUrl).HasMaxLength(500);
        b.Property(x => x.Models).HasMaxLength(50);
        b.Property(x => x.Reason).HasMaxLength(500);
        b.Property(x => x.ReviewNote).HasMaxLength(500);

        b.HasOne(x => x.Camera).WithMany()
         .HasForeignKey(x => x.CameraId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Department).WithMany()
         .HasForeignKey(x => x.DepartmentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.RequestedBy).WithMany()
         .HasForeignKey(x => x.RequestedById)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ReviewedBy).WithMany()
         .HasForeignKey(x => x.ReviewedById)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
