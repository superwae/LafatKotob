using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class HistoryConfiguration : IEntityTypeConfiguration<History>
    {
        public void Configure(EntityTypeBuilder<History> builder)
        {
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Date).IsRequired();
;

            // Configuring the one-to-one relationship with AppUser
            builder.HasOne(h => h.AppUser)
                   .WithOne(u => u.History)
                   .HasForeignKey<History>(h => h.UserId)
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
