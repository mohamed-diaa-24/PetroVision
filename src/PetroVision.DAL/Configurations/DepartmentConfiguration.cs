using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.Name).IsUnique();
    }
}
