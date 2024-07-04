using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class BookPostCommentModel
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public string UserId { get; set; }

        public string CommentText { get; set; }
        public DateTime DateCommented { get; set; }
    }
}
