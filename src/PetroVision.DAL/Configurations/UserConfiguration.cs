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

        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.EmployeeCode).IsUnique();
    }
}
