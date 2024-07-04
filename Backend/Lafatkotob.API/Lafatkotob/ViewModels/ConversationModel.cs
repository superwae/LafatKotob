namespace Lafatkotob.ViewModels
{
    public class ConversationModel
    {
        public int Id { get; set; }
        public string LastMessage { get; set; }

        public DateTime LastMessageDate { get; set; }
        public bool HasUnreadMessages { get; set; }
        public string? LastMessageSender { get; set; }

    }
}
