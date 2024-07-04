using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Book
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string CoverImage { get; set; }

    [ForeignKey(nameof(AppUser))]
    public string UserId { get; set; }
    [ForeignKey(nameof(History))]
    public int? HistoryId { get; set; } 
    public DateTime PublicationDate { get; set; }
    public string ISBN { get; set; }
    public int? PageCount { get; set; }
    public string Condition { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public string Language { get; set; }
    public DateTime AddedDate { get; set; }

    // Foreign key to the user who currently has the book or is interested in it
    [ForeignKey(nameof(PartnerUserId))]
    public string PartnerUserId { get; set; }

    // Navigation properties
    public virtual AppUser AppUser { get; set; }
    public virtual AppUser PartnerAppUser { get; set; }
    public virtual History History { get; set; }
    public virtual ICollection<BookGenre> BookGenres { get; set; }
    public virtual ICollection<BookPostLike> BookPostLikes { get; set; }
    public virtual ICollection<BookPostComment> BookPostComments { get; set; }


    public Book()
    {
        BookGenres = new HashSet<BookGenre>();
        BookPostLikes = new HashSet<BookPostLike>();
        BookPostComments = new HashSet<BookPostComment>();
    }
}
