namespace Lafatkotob.ViewModels
{
    public class ConversationsBoxModel
    {
        public int ConversationId { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageDate { get; set; }
        public string UserId { get; set; } 
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public bool HasUnreadMessages { get; set; }
        public string? LastMessageSender { get; set; }
    }
}
