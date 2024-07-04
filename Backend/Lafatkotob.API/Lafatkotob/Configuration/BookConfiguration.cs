using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title).IsRequired().HasMaxLength(255);
            builder.Property(b => b.Author).IsRequired().HasMaxLength(255);
            builder.Property(b => b.Description).HasMaxLength(1000);
            builder.Property(b => b.CoverImage).IsRequired().HasMaxLength(2048);
            builder.Property(b => b.PublicationDate).IsRequired();
            builder.Property(b => b.ISBN).HasMaxLength(13);
            builder.Property(b => b.PageCount);
            builder.Property(b => b.Condition).IsRequired().HasMaxLength(100);
            builder.Property(b => b.Status).IsRequired().HasMaxLength(50);

            // Relationship with AppUser (owner of the book)
            builder.HasOne(b => b.AppUser)
                   .WithMany(u => u.Books)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relationship with PartnerAppUser (user who acquired the book)
            builder.HasOne(b => b.PartnerAppUser)
                   .WithMany(u=>u.PartnerBooks) 
                   .HasForeignKey(b => b.PartnerUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Configuring the one-to-many relationship with History
            builder.HasOne(b => b.History)
                   .WithMany(h => h.Books)
                   .HasForeignKey(b => b.HistoryId)
                   .OnDelete(DeleteBehavior.Restrict); 

            // Many-to-Many: BookGenres (via BookGenre entity)
            builder.HasMany(b => b.BookGenres)
                   .WithOne(bg => bg.Book)
                   .HasForeignKey(bg => bg.BookId)
                   .OnDelete(DeleteBehavior.Cascade); 

            // One-to-Many: BookPostLikes
            builder.HasMany(b => b.BookPostLikes)
                   .WithOne(bl => bl.Book)
                   .HasForeignKey(bl => bl.BookId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: BookPostComments
            builder.HasMany(b => b.BookPostComments)
                   .WithOne(bc => bc.Book)
                   .HasForeignKey(bc => bc.BookId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
