using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.Entities
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }
        public string? LastMessage { get; set; }

        public DateTime LastMessageDate { get; set; }
        public bool HasUnreadMessages { get; set; }
        public string ? LastMessageSender { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<ConversationsUser> ConversationsUsers { get; set; }


        public Conversation()
        {
            Messages = new HashSet<Message>();
            ConversationsUsers = new HashSet<ConversationsUser>(); 
        }
    }
}
