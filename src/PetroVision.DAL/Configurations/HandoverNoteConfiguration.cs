using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroVision.DAL.Entities;

namespace PetroVision.DAL.Configurations;

public class HandoverNoteConfiguration : IEntityTypeConfiguration<HandoverNote>
{
    public void Configure(EntityTypeBuilder<HandoverNote> b)
    {
        b.Property(x => x.Title).IsRequired().HasMaxLength(150);
        b.Property(x => x.Details).IsRequired().HasMaxLength(2000);

        b.HasOne(x => x.Department).WithMany()
         .HasForeignKey(x => x.DepartmentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Author).WithMany()
         .HasForeignKey(x => x.AuthorId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HandoverReadConfiguration : IEntityTypeConfiguration<HandoverRead>
{
    public void Configure(EntityTypeBuilder<HandoverRead> b)
    {
        b.HasIndex(x => new { x.HandoverNoteId, x.UserId }).IsUnique();

        b.HasOne(x => x.HandoverNote)
         .WithMany(x => x.Reads)
         .HasForeignKey(x => x.HandoverNoteId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.User).WithMany()
         .HasForeignKey(x => x.UserId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
