using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class BookPostCommentConfiguration : IEntityTypeConfiguration<BookPostComment>
    {
        public void Configure(EntityTypeBuilder<BookPostComment> builder)
        {

            builder.HasKey(bpc => bpc.Id);

            builder.Property(bpc => bpc.CommentText).IsRequired();
            builder.Property(bpc => bpc.DateCommented).IsRequired();

            // Configuring the relationship with Book
            builder.HasOne(bpc => bpc.Book)
                   .WithMany(b => b.BookPostComments)
                   .HasForeignKey(bpc => bpc.BookId)
                   .OnDelete(DeleteBehavior.Restrict); 

            // Configuring the relationship with AppUser

            builder.HasOne(bpc => bpc.AppUser)
                   .WithMany(u => u.BookPostComments)
                   .HasForeignKey(bpc => bpc.UserId)
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
