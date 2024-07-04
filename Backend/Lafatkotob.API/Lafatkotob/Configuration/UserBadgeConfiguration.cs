using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class UserBadgeConfiguration : IEntityTypeConfiguration<UserBadge>
    {
        public void Configure(EntityTypeBuilder<UserBadge> builder)
        {
            builder.HasKey(ub => ub.Id);
            builder.Property(ub => ub.DateEarned).IsRequired();

            // Configuring the relationship between UserBadge and AppUser
            builder.HasOne(ub => ub.AppUser)
                   .WithMany(u => u.UserBadges) 
                   .HasForeignKey(ub => ub.UserId)
                   .OnDelete(DeleteBehavior.Cascade); 

            // Configuring the relationship between UserBadge and Badge
            builder.HasOne(ub => ub.Badge)
                   .WithMany(b => b.UserBadges)
                   .HasForeignKey(ub => ub.BadgeId)
                   .OnDelete(DeleteBehavior.Cascade); 

        }
    }
}
