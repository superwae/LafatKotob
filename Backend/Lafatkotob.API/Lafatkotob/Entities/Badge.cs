using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.Entities
{
    public class Badge
    {
        [Key]
        public int Id { get; set; }
        public string BadgeName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<UserBadge> UserBadges { get; set; }

        public Badge()
        {
            UserBadges = new HashSet<UserBadge>();
        }
    }
}
