using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; } 
        public DateTime DateScheduled { get; set; }
        public string Location { get; set; }
        public string? SubLocation { get; set; }
        public int attendances { get; set; }
        public string ImagePath { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string HostUserId { get; set; }

        public virtual AppUser HostUser { get; set; }
        public virtual ICollection<UserEvent> UserEvents { get; set; }

        public Event()
        {
            UserEvents = new HashSet<UserEvent>();
        }
    }
}
