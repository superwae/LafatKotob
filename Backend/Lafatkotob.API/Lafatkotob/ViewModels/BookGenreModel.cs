using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class BookGenreModel
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public int GenreId { get; set; }

    }
}
