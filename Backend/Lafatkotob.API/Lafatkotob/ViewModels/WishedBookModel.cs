using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class WishedBookModel
    {
        public int Id { get; set; }

       
        public int BooksInWishlistsId { get; set; }

      
        public int WishlistId { get; set; }
    }
}
