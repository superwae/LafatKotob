using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class ConversationsUser
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; }

        [ForeignKey(nameof(Conversation))]
        public int ConversationId { get; set; }

        public virtual AppUser AppUser { get; set; }
        public virtual Conversation Conversation { get; set; }
    }
}
