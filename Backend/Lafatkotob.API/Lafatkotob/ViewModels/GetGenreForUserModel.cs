namespace Lafatkotob.ViewModels
{
    public class GetGenreForUserModel
    {
        public string user_id { get; set; }
        public List<BookGenres> books { get; set; }
    }

    public class BookGenres
    {
        public List<string> genres { get; set; } = new List<string>();
    }
}
