using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class NotificationUserConfiguration : IEntityTypeConfiguration<NotificationUser>
    {
        public void Configure(EntityTypeBuilder<NotificationUser> builder)
        {
            builder.HasKey(nu => nu.Id);

            // Configuring the many-to-many relationship between AppUser and Notification
            builder.HasOne(nu => nu.AppUser)
                   .WithMany(u => u.NotificationsUsers) 
                   .HasForeignKey(nu => nu.UserId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(nu => nu.Notification)
                   .WithMany(n => n.NotificationUsers) 
                   .HasForeignKey(nu => nu.NotificationId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
