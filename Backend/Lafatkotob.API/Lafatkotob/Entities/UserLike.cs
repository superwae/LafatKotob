using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class UserLike
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(LikedAppUser))]
        public string LikedUserId { get; set; }

        [ForeignKey(nameof(LikingAppUser))]
        public string LikingUserId { get; set; }

        public DateTime DateLiked { get; set; }

        public virtual AppUser LikedAppUser { get; set; } 
        public virtual AppUser LikingAppUser { get; set; } 
    }
}
