using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        b.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(20);
        b.Property(x => x.Email).IsRequired().HasMaxLength(150);
        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.SecurityStamp).IsRequired().HasMaxLength(64);
        b.Property(x => x.StatusReason).HasMaxLength(500);
        b.Property(x => x.PasswordResetCodeHash).HasMaxLength(100);

        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.EmployeeCode).IsUnique();

        b.HasOne(x => x.Department).WithMany()
         .HasForeignKey(x => x.DepartmentId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
