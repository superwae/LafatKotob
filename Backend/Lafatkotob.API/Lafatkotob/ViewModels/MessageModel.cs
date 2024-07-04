using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class MessageModel
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public string SenderUserId { get; set; }

        public string ReceiverUserId { get; set; }

        public string MessageText { get; set; }
        public DateTime DateSent { get; set; }
        public bool IsReceived { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeletedBySender { get; set; }
        public bool IsDeletedByReceiver { get; set; }
    }
}
