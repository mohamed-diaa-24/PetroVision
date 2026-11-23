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

        // Names are unique inside a department; removed cameras free their name.
        b.HasIndex(x => new { x.DepartmentId, x.Name })
         .IsUnique()
         .HasFilter("[IsDeleted] = 0");

        b.HasOne(x => x.Department).WithMany()
         .HasForeignKey(x => x.DepartmentId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
