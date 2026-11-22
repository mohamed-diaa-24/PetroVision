using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> b)
    {
        b.Property(x => x.Title).IsRequired().HasMaxLength(150);
        b.Property(x => x.Message).IsRequired().HasMaxLength(2000);

        b.HasOne(x => x.Department).WithMany()
         .HasForeignKey(x => x.DepartmentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Author).WithMany()
         .HasForeignKey(x => x.AuthorId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AnnouncementReadConfiguration : IEntityTypeConfiguration<AnnouncementRead>
{
    public void Configure(EntityTypeBuilder<AnnouncementRead> b)
    {
        b.HasIndex(x => new { x.AnnouncementId, x.UserId }).IsUnique();

        b.HasOne(x => x.Announcement)
         .WithMany(x => x.Reads)
         .HasForeignKey(x => x.AnnouncementId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.User).WithMany()
         .HasForeignKey(x => x.UserId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
