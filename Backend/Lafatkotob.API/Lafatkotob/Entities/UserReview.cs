using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class UserReview
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(ReviewedAppUser))]
        public string ReviewedUserId { get; set; }

        [ForeignKey(nameof(ReviewingAppUser))]
        public string ReviewingUserId { get; set; }

        public string ReviewText { get; set; } 
        public DateTime DateReviewed { get; set; }
        public int Rating { get; set; }

        public virtual AppUser ReviewedAppUser { get; set; }
        public virtual AppUser ReviewingAppUser { get; set; }
    }
}
