using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {

            builder.HasKey(c => c.Id);

            builder.Property(c => c.LastMessageDate).IsRequired();

            // Configuring the one-to-many relationship with Message
            builder.HasMany(c => c.Messages)
                   .WithOne(m => m.Conversation)
                   .HasForeignKey(m=> m.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade);
            // whith conversationUser many to one 
            builder.HasMany(cu => cu.ConversationsUsers)
                  .WithOne(u => u.Conversation)
                  .HasForeignKey(cu => cu.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);



        }
    }
}
