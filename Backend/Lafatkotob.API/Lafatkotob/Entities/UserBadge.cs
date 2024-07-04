using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class UserBadge
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; }

        [ForeignKey(nameof(Badge))]
        public int BadgeId { get; set; }

        public DateTime DateEarned { get; set; }

        public virtual AppUser AppUser { get; set; } 
        public virtual Badge Badge { get; set; } 
    }
}
