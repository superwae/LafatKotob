using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class UserEvent
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; }

        [ForeignKey(nameof(Event))]
        public int EventId { get; set; }

        public virtual AppUser AppUser { get; set; }
        public virtual Event Event { get; set; }
    }
}
