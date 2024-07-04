using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class WishedBookConfiguration : IEntityTypeConfiguration<WishedBook>
    {
        public void Configure(EntityTypeBuilder<WishedBook> builder)
        {
            builder.HasKey(wb => wb.Id);


            // Configuring the relationship between WishedBook and Wishlist
            builder.HasOne(wb => wb.Wishlist)
                   .WithMany(wl => wl.WishedBooks) 
                   .HasForeignKey(wb => wb.WishlistId)
                   .OnDelete(DeleteBehavior.Restrict); 

            // Configuring the relationship between WishedBook and BooksInWishlists
            builder.HasOne(wb => wb.BooksInWishlists)
                   .WithMany(biw => biw.WishedBooks) 
                   .HasForeignKey(wb => wb.BooksInWishlistsId)
                   .OnDelete(DeleteBehavior.Restrict); 

        }
    }
}
