using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class BookPostLikeConfiguration : IEntityTypeConfiguration<BookPostLike>
    {
        public void Configure(EntityTypeBuilder<BookPostLike> builder)
        {

            builder.HasKey(bpl => bpl.Id);

            builder.Property(bpl => bpl.DateLiked).IsRequired();

            builder.HasIndex(bpl => new { bpl.UserId, bpl.BookId }).IsUnique();

            // Configuring the relationship with Book
            builder.HasOne(bpl => bpl.Book)
                    .WithMany(b => b.BookPostLikes)
                    .HasForeignKey(bpl => bpl.BookId)
                    .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship with AppUser
            builder.HasOne(bpl => bpl.AppUser)
                   .WithMany(u => u.BookPostLikes)
                   .HasForeignKey(bpl => bpl.UserId)
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
