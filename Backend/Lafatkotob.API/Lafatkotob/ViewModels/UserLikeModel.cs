using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class UserLikeModel
    {
        public int Id { get; set; }

      
        public string LikedUserId { get; set; }

       
        public string LikingUserId { get; set; }

        public DateTime DateLiked { get; set; }
    }
}
