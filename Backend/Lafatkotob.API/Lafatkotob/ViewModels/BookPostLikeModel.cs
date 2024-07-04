using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class BookPostLikeModel
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public string UserId { get; set; }

        public DateTime DateLiked { get; set; }
    }
}
