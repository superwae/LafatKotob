using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class ConversationsUserConfiguration : IEntityTypeConfiguration<ConversationsUser>
    {
        public void Configure(EntityTypeBuilder<ConversationsUser> builder)
        {

            builder.HasKey(cu => cu.Id);

            // Many-to-Many: AppUser <-> Conversation

            // Relationship with AppUser
            builder.HasOne(cu => cu.AppUser)
                   .WithMany(u => u.ConversationsUsers)
                   .HasForeignKey(cu => cu.UserId)
                   .OnDelete(DeleteBehavior.Cascade); 

            // Relationship with Conversation
            builder.HasOne(cu => cu.Conversation)
                   .WithMany(u => u.ConversationsUsers)
                   .HasForeignKey(cu => cu.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
