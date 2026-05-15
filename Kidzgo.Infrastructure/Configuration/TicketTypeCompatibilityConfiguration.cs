using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TicketTypeCompatibilityConfiguration : IEntityTypeConfiguration<TicketTypeCompatibility>
{
    public void Configure(EntityTypeBuilder<TicketTypeCompatibility> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LearningTicketTypeId)
            .IsRequired();

        builder.Property(x => x.SlotTypeId)
            .IsRequired();

        builder.Property(x => x.IsCompatible)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.LearningTicketType)
            .WithMany(x => x.Compatibilities)
            .HasForeignKey(x => x.LearningTicketTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SlotType)
            .WithMany()
            .HasForeignKey(x => x.SlotTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LearningTicketTypeId, x.SlotTypeId })
            .IsUnique();
    }
}
