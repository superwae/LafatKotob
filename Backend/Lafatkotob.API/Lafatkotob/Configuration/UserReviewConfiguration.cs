using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class UserReviewConfiguration : IEntityTypeConfiguration<UserReview>
    {
        public void Configure(EntityTypeBuilder<UserReview> builder)
        {
            builder.HasKey(ur => ur.Id);
            builder.Property(ur => ur.ReviewText).HasMaxLength(1000);
            builder.Property(ur => ur.DateReviewed).IsRequired();
            builder.Property(ur => ur.Rating).IsRequired();

            // Configuring the relationship for the user being reviewed
            builder.HasOne(ur => ur.ReviewedAppUser)
                   .WithMany(u => u.UserReviewed) 
                   .HasForeignKey(ur => ur.ReviewedUserId)
                   .OnDelete(DeleteBehavior.Restrict); 

            // Configuring the relationship for the user who is reviewing
            builder.HasOne(ur => ur.ReviewingAppUser)
                   .WithMany(u => u.UserReviews) 
                   .HasForeignKey(ur => ur.ReviewingUserId)
                   .OnDelete(DeleteBehavior.Restrict); 

          
        }
    }
}
