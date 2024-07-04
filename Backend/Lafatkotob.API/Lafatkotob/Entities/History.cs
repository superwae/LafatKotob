using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class History
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AppUser")]
        public string UserId { get; set; } 


        public DateTime Date { get; set; }


        public virtual AppUser AppUser { get; set; }
        public virtual ICollection<Book> Books { get; set; }

        public History()
        {
            Books = new HashSet<Book>();
        }
    }
}
