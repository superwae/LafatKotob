namespace Lafatkotob.ViewModels
{
    public class ConversationWithIdsModel
    {
        public int Id { get; set; }
        public List<string> UserIds { get; set; }
        public string? LastMessage { get; set; }
        public DateTime LastMessageDate { get; set; }
        public string? LastMessageSender { get; set; }


    }
}
