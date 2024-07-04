using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class UserEventModel
    {
        public int Id { get; set; }

     
        public string UserId { get; set; }

      
        public int EventId { get; set; }
    }
}
