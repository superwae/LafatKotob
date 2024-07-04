using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class BookGenreConfiguration : IEntityTypeConfiguration<BookGenre>
    {
        public void Configure(EntityTypeBuilder<BookGenre> builder)
        {

            builder.HasKey(bg => bg.Id);

            // Configure the foreign key for Book
            builder.HasOne(bg => bg.Book)
           .WithMany(b => b.BookGenres)
           .HasForeignKey(bg => bg.BookId)
           .OnDelete(DeleteBehavior.Cascade);
            

            // Configure the foreign key for Genre
            builder.HasOne(bg => bg.Genre)
                   .WithMany(g => g.BookGenres)
                   .HasForeignKey(bg => bg.GenreId)
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
