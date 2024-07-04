namespace Lafatkotob.ViewModels
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime DateSent { get; set; }
        public bool IsRead { get; set; }
        public string UserId {  get; set; }

        public string UserName { get; set; }

        public string ImgUrl { get; set; }
    }
}
