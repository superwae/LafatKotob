using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class BookPostLike
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; } 

        public DateTime DateLiked { get; set; }

        public virtual Book Book { get; set; }
        public virtual AppUser AppUser { get; set; } 
    }
}
