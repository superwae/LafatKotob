using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Conversation))]
        public int ConversationId { get; set; }

        [ForeignKey(nameof(SenderUser))]
        public string SenderUserId { get; set; }

        [ForeignKey(nameof(ReceiverUser))]
        public string ReceiverUserId { get; set; }

        public string MessageText { get; set; }
        public DateTime DateSent { get; set; }
        public bool IsReceived { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeletedBySender { get; set; }
        public bool IsDeletedByReceiver { get; set; }

        public virtual Conversation Conversation { get; set; }
        public virtual AppUser SenderUser { get; set; }
        public virtual AppUser ReceiverUser { get; set; }
    }
}
