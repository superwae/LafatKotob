using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class EventModel
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime DateScheduled { get; set; }
        public string Location { get; set; }
        public string ImagePath { get; set; }
        public string HostUserId { get; set; }
        public int attendances { get; set; }
        public string? SubLocation { get; set; }
    }
}
