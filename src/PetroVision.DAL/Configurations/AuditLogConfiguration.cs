using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
        b.Property(x => x.Item).IsRequired().HasMaxLength(150);
        b.Property(x => x.Details).HasMaxLength(500);

        b.HasIndex(x => x.CreatedAt);

        b.HasOne(x => x.User).WithMany()
         .HasForeignKey(x => x.UserId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Department).WithMany()
         .HasForeignKey(x => x.DepartmentId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
