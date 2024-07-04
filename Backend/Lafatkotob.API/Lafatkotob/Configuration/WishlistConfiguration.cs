using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.HasKey(wl => wl.Id);
            builder.Property(wl => wl.DateAdded).IsRequired();

            // Configuring the one-to-one relationship between Wishlist and AppUser
            builder.HasOne(wl => wl.AppUser)
                   .WithOne(u => u.Wishlist) 
                   .HasForeignKey<Wishlist>(wl => wl.UserId)
                   .OnDelete(DeleteBehavior.Cascade);


            // Configuring the one-to-many relationship between Wishlist and WishedBook
            builder.HasMany(wl => wl.WishedBooks)
                   .WithOne(wb => wb.Wishlist)
                   .HasForeignKey(wb => wb.WishlistId)
                   .OnDelete(DeleteBehavior.Cascade); 

        }
    }
}
