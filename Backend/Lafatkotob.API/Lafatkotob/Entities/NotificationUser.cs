using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class NotificationUser
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; }

        [ForeignKey(nameof(Notification))]
        public int NotificationId { get; set; }

        public virtual AppUser AppUser { get; set; }
        public virtual Notification Notification { get; set; }
    }
}
