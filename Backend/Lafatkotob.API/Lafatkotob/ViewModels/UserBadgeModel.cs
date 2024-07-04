using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class UserBadgeModel
    {
        public int Id { get; set; }

       
        public string UserId { get; set; }

       
        public int BadgeId { get; set; }

        public DateTime DateEarned { get; set; }
    }
}
