using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class BooksInWishlistsConfiguration : IEntityTypeConfiguration<BooksInWishlists>
    {
        public void Configure(EntityTypeBuilder<BooksInWishlists> builder)
        {

            builder.HasKey(biw => biw.Id);

            builder.Property(biw => biw.Title).IsRequired().HasMaxLength(255);
            builder.Property(biw => biw.Author).IsRequired().HasMaxLength(255);
            builder.Property(biw => biw.ISBN).HasMaxLength(13); 

            // Configuring the one-to-many relationship with WishedBook
            builder.HasMany(biw => biw.WishedBooks)
                   .WithOne(wb => wb.BooksInWishlists)
                   .HasForeignKey(wb => wb.BooksInWishlistsId)
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
