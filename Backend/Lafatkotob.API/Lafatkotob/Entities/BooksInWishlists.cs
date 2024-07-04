using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.Entities
{
    public class BooksInWishlists
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Language { get; set; }
        public DateTime AddedDate { get; set; }

        public virtual ICollection<WishedBook> WishedBooks { get; set; }

        public BooksInWishlists()
        {
            WishedBooks = new HashSet<WishedBook>();
        }
    }
}
