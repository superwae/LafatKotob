using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
    {
        public void Configure(EntityTypeBuilder<UserPreference> builder)
        {
            builder.HasKey(up => up.Id);
            builder.Property(up => up.PreferredAuthor).HasMaxLength(255);
            // Configuring the relationship between UserPreference and AppUser
            builder.HasOne(up => up.AppUser)
                   .WithMany(u => u.UserPreferences) 
                   .HasForeignKey(up => up.UserId)
                   .OnDelete(DeleteBehavior.Restrict); 

            // Configuring the relationship between UserPreference and Genre
            builder.HasOne(up => up.Genre)
                   .WithMany(g => g.UserPreferences) 
                   .HasForeignKey(up => up.GenreId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
