using Lafatkotob.Entities;
using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.ConversationService
{
    public interface IConversationService
    {
        Task<ServiceResponse<ConversationModel>> Post(ConversationModel model);
        Task<ConversationModel> GetById(int id);
        Task<List<ConversationModel>> GetAll();
        Task<ServiceResponse<ConversationModel>> Update(ConversationModel model);
        Task<ServiceResponse<ConversationModel>>Delete(int id);
        Task<ServiceResponse<List<ConversationsBoxModel>>> GetConversationsForUser(string userId);
        Task<ServiceResponse<List<Message>>> GetAllMessagesByConversationIdAsync(int conversationId, int pageNumber = 1, int pageSize = 20);
        Task<ServiceResponse<bool>> PostNewConversation(ConversationWithIdsModel model);

        Task<ServiceResponse<List<ConversationsBoxModel>>> getUsersForConversation(int id);
        Task<ServiceResponse<int>>ConversationCountWithUnreadMessages(string userId);

        Task<ServiceResponse<ConversationModel>> MarkConversationAsRead(int conversationId, string userId);

    }
}
