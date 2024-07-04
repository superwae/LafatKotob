using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lafatkotob.Entities;

namespace Lafatkotob.Configuration
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.MessageText).IsRequired();
            builder.Property(m => m.DateSent).IsRequired();
            builder.Property(m => m.IsReceived).IsRequired();
            builder.Property(m => m.IsRead).IsRequired();
            builder.Property(m => m.IsDeletedBySender).IsRequired();
            builder.Property(m => m.IsDeletedByReceiver).IsRequired();

            // Configuring the relationship with Conversation
            builder.HasOne(m => m.Conversation)
                   .WithMany(c => c.Messages)
                   .HasForeignKey(m => m.ConversationId)
                   .OnDelete(DeleteBehavior.Restrict); 

            // Configuring the relationships with SenderUser and ReceiverUser
            builder.HasOne(m => m.SenderUser)
                   .WithMany(u=>u.MessagesSent) 
                   .HasForeignKey(m => m.SenderUserId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(m => m.ReceiverUser)
                   .WithMany(u=>u.MessagesReceived) 
                   .HasForeignKey(m => m.ReceiverUserId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
