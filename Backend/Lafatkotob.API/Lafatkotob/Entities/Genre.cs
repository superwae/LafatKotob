using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.Entities
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<BookGenre> BookGenres { get; set; }
        public virtual ICollection<UserPreference> UserPreferences { get; set; }

        public Genre()
        {
            BookGenres = new HashSet<BookGenre>();
            UserPreferences = new HashSet<UserPreference>();
        }
    }
}
