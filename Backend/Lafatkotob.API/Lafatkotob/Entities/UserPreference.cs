using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class UserPreference
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AppUser))]
        public string UserId { get; set; }

        [ForeignKey(nameof(Genre))]
        public int GenreId { get; set; }

        public string PreferredAuthor { get; set; }

        public virtual AppUser AppUser { get; set; } 
        public virtual Genre Genre { get; set; }
    }
}
