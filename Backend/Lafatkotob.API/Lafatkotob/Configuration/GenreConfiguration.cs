using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class GenreConfiguration : IEntityTypeConfiguration<Genre>
    {
        public void Configure(EntityTypeBuilder<Genre> builder)
        {

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name).IsRequired().HasMaxLength(255);

            // Configuring the one-to-many relationship with BookGenre
            builder.HasMany(g => g.BookGenres)
                   .WithOne(bg => bg.Genre)
                   .HasForeignKey(bg => bg.GenreId)
                   .OnDelete(DeleteBehavior.Cascade); 

            // Configuring the one-to-many relationship with UserPreference
            builder.HasMany(g => g.UserPreferences)
                   .WithOne(up => up.Genre)
                   .HasForeignKey(up => up.GenreId)
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
