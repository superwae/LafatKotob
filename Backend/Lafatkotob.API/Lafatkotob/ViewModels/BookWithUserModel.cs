namespace Lafatkotob.ViewModels
{
    public class BookWithUserModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string CoverImage { get; set; }
        public string UserId { get; set; }
        public int? HistoryId { get; set; }
        public DateTime PublicationDate { get; set; }
        public string ISBN { get; set; }
        public int? PageCount { get; set; }
        public string Condition { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string PartnerUserId { get; set; }
        public string Language { get; set; }
        public DateTime AddedDate { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
    }
}
