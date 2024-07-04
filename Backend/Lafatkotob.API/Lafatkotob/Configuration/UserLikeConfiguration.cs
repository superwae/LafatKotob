using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class UserLikeConfiguration : IEntityTypeConfiguration<UserLike>
    {
        public void Configure(EntityTypeBuilder<UserLike> builder)
        {
            builder.HasKey(ul => ul.Id);

            // Configuring the relationship for the user who is liked
            builder.HasOne(ul => ul.LikedAppUser)
                   .WithMany(u => u.UserLiked) 
                   .HasForeignKey(ul => ul.LikedUserId)
                   .OnDelete(DeleteBehavior.Restrict); 

            // Configuring the relationship for the user who is liking
            builder.HasOne(ul => ul.LikingAppUser)
                   .WithMany(u => u.UserLikes)
                   .HasForeignKey(ul => ul.LikingUserId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(ul => ul.DateLiked).IsRequired();
        }
    }
}
