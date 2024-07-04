using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class UserReviewModel
    {
        public int Id { get; set; }

      
        public string ReviewedUserId { get; set; }

     
        public string ReviewingUserId { get; set; }

        public string ReviewText { get; set; }
        public DateTime DateReviewed { get; set; }
        public int Rating { get; set; }
    }
}
