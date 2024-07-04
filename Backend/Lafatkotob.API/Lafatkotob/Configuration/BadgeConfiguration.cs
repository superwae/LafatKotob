using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
    {
        public void Configure(EntityTypeBuilder<Badge> builder)
        {

            builder.HasKey(b => b.Id);

            builder.Property(b => b.BadgeName)
                   .IsRequired()
                   .HasMaxLength(256); 

            builder.Property(b => b.Description)
                   .HasMaxLength(1000); 

            // Configuring the one-to-many relationship with UserBadge
            builder.HasMany(b => b.UserBadges)
                   .WithOne(ub => ub.Badge)
                   .HasForeignKey(ub => ub.BadgeId);
        }
    }
}
