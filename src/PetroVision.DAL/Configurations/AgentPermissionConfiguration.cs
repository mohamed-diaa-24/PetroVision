using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class AgentPermissionConfiguration : IEntityTypeConfiguration<AgentPermission>
{
    public void Configure(EntityTypeBuilder<AgentPermission> b)
    {
        b.HasIndex(x => new { x.UserId, x.Permission }).IsUnique();

        b.HasOne(x => x.User)
         .WithMany(u => u.Permissions)
         .HasForeignKey(x => x.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
