using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.Entities
{
    public class WishedBook
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(BooksInWishlists))]
        public int BooksInWishlistsId { get; set; }

        [ForeignKey(nameof(Wishlist))]
        public int WishlistId { get; set; }

        public virtual Wishlist Wishlist { get; set; }
        public virtual BooksInWishlists BooksInWishlists { get; set; }
    }
}
