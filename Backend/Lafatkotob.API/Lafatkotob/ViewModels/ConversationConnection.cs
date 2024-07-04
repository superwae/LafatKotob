namespace Lafatkotob.ViewModels
{
    public class ConversationConnection
    {
        public string SenderId { get; set; }
        public string ReciverId { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageDate { get; set; }
        public int ConversationId { get; set; }
        
    }
}
