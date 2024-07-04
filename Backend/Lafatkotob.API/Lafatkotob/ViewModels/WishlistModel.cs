using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class WishlistModel
    {
        public int Id { get; set; }
      
        public string UserId { get; set; }

        public DateTime DateAdded { get; set; }

       
    }
}
