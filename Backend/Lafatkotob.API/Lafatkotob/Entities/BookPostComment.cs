using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class BookPostComment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; } 

        public string CommentText { get; set; }
        public DateTime DateCommented { get; set; }

        public virtual Book Book { get; set; }
        public virtual AppUser AppUser { get; set; }
    }
}
