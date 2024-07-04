using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime DateSent { get; set; }
        public bool IsRead { get; set; }
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string imgUrl { get; set; }
        public virtual ICollection<NotificationUser> NotificationUsers { get; set; }

        public Notification()
        {
            NotificationUsers = new HashSet<NotificationUser>();
        }
    }
}
