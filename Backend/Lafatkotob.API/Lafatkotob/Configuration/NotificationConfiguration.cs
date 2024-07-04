using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Message).IsRequired();
            builder.Property(n => n.DateSent).IsRequired();
            builder.Property(n => n.IsRead).IsRequired();

            builder.HasMany(n => n.NotificationUsers)
                   .WithOne(nu => nu.Notification)
                   .HasForeignKey(nu => nu.NotificationId).OnDelete(DeleteBehavior.Cascade);

        }
    }
}
