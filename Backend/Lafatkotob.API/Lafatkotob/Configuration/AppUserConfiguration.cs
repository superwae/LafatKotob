using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // Standard properties
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.Property(u => u.NormalizedEmail).IsRequired().HasMaxLength(256);
            builder.Property(u => u.UserName).IsRequired().HasMaxLength(256);
            builder.Property(u => u.NormalizedUserName).IsRequired().HasMaxLength(256);
            builder.Property(u => u.EmailConfirmed).IsRequired();
            builder.Property(u => u.SecurityStamp).IsRequired();
            builder.Property(u => u.ConcurrencyStamp).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.PhoneNumber).IsRequired(false);

            //unique properties
            builder.HasIndex(u => u.UserName).IsUnique();

            // Custom properties
            builder.Property(u => u.IsDeleted).IsRequired();
            builder.Property(u => u.City).IsRequired(false).HasMaxLength(255);
            builder.Property(u => u.DateJoined).IsRequired();
            builder.Property(u => u.LastLogin).IsRequired();
            builder.Property(u => u.ProfilePicture).IsRequired(false).HasMaxLength(2048);
            builder.Property(u => u.About).IsRequired(false).HasMaxLength(1000);
            builder.Property(u => u.DTHDate).IsRequired();

            // Ignore the calculated Age property
            builder.Ignore(u => u.Age);

            // Relationships

            // Books 
            builder.HasMany(u => u.Books)
                   .WithOne(b => b.AppUser)
                   .HasForeignKey(b => b.UserId);

            // UserPreferences
            builder.HasMany(u => u.UserPreferences)
                   .WithOne(p => p.AppUser)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);



            // BookPostLikes
            builder.HasMany(u => u.BookPostLikes)
                   .WithOne(l => l.AppUser)
                   .HasForeignKey(l => l.UserId)
                   .OnDelete(DeleteBehavior.Cascade);



            // BookPostComments
            builder.HasMany(u => u.BookPostComments)
                   .WithOne(c => c.AppUser)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // UserLikes - Configuring the self-referencing relationship
            builder.HasMany(u => u.UserLikes)
                   .WithOne(l => l.LikingAppUser)
                   .HasForeignKey(l => l.LikingUserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.UserLiked)
                   .WithOne(l => l.LikedAppUser)
                   .HasForeignKey(l => l.LikedUserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // UserReviews
            builder.HasMany(u => u.UserReviews)
                   .WithOne(r => r.ReviewingAppUser)
                   .HasForeignKey(r => r.ReviewingUserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.UserReviewed)
                   .WithOne(r => r.ReviewedAppUser)
                   .HasForeignKey(r => r.ReviewedUserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // UserEvents
            builder.HasMany(u => u.UserEvents)
                   .WithOne(e => e.AppUser)
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // UserBadges
            builder.HasMany(u => u.UserBadges)
                   .WithOne(b => b.AppUser)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ConversationsUsers
            builder.HasMany(u => u.ConversationsUsers)
                   .WithOne(c => c.AppUser)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Messages
            builder.HasMany(u => u.MessagesSent)
                   .WithOne(m => m.SenderUser)
                   .HasForeignKey(m => m.SenderUserId)
                   .OnDelete(DeleteBehavior.Cascade);
            // Messages
            builder.HasMany(u => u.MessagesReceived)
                   .WithOne(m => m.ReceiverUser)
                   .HasForeignKey(m => m.ReceiverUserId)
                   .OnDelete(DeleteBehavior.Cascade);
            //history
            builder.HasOne(h => h.History)
                  .WithOne(u => u.AppUser)
                  .HasForeignKey<History>(u => u.UserId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .OnDelete(DeleteBehavior.Cascade);

            // NotificationsUsers
            builder.HasMany(u => u.NotificationsUsers)
                   .WithOne(n => n.AppUser)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Configuring a one-to-one relationship with Wishlist
            builder.HasOne(u => u.Wishlist)
                   .WithOne(w => w.AppUser)
                   .HasForeignKey<Wishlist>(w => w.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            //configuring the one-to-one relationship with History
            builder.HasOne(u => u.History)
                   .WithOne(h => h.AppUser)
                   .HasForeignKey<History>(h => h.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.GivenVotes)
                   .WithOne(v => v.VoterUser)
                   .HasForeignKey(v => v.VoterUserId)
                   .OnDelete(DeleteBehavior.Restrict); 
            builder.HasMany(u => u.ReceivedVotes)
                   .WithOne(v => v.TargetUser)
                   .HasForeignKey(v => v.TargetUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
