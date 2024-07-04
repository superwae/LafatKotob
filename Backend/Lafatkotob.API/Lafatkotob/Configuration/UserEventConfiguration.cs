using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class UserEventConfiguration : IEntityTypeConfiguration<UserEvent>
    {
        public void Configure(EntityTypeBuilder<UserEvent> builder)
        {
            builder.HasKey(ue => ue.Id);

            // Configuring the relationship between UserEvent and AppUser
            builder.HasOne(ue => ue.AppUser)
                   .WithMany(u => u.UserEvents) 
                   .HasForeignKey(ue => ue.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship between UserEvent and Event
            builder.HasOne(ue => ue.Event)
                   .WithMany(e => e.UserEvents) 
                   .HasForeignKey(ue => ue.EventId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
