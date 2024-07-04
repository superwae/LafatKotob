using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class BookGenre
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }

        [ForeignKey(nameof(Genre))]
        public int GenreId { get; set; }

        public virtual Book Book { get; set; }
        public virtual Genre Genre { get; set; }
    }
}
