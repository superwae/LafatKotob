using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class UserPreferenceModel
    {
        public int Id { get; set; }

     
        public string UserId { get; set; }

   
        public int GenreId { get; set; }

        public string PreferredAuthor { get; set; }
    }
}
