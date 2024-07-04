using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
           
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EventName).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Description).HasMaxLength(1000); 
            builder.Property(e => e.DateScheduled).IsRequired();
            builder.Property(e => e.Location).IsRequired().HasMaxLength(255);

            // Configuring the relationship with AppUser as the event host
            builder.HasOne(e => e.HostUser)
                   .WithMany(u=>u.Events) 
                   .HasForeignKey(e => e.HostUserId)
                   .OnDelete(DeleteBehavior.Restrict); 


        }
    }
}
