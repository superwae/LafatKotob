using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; }

        public DateTime DateAdded { get; set; }

        public virtual AppUser AppUser { get; set; }
        public virtual ICollection<WishedBook> WishedBooks { get; set; } 

        public Wishlist()
        {
            WishedBooks = new HashSet<WishedBook>();
        }
    }
}






