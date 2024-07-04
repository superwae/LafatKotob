using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.MessageService
{
    public interface IMessageService
    {
        Task<ServiceResponse<ConversationModel>> Post(MessageModel model);
        Task<MessageModel> GetById(int id);
        Task<List<MessageModel>> GetAll();
        Task<ServiceResponse<MessageModel>> Update(MessageModel model);
        Task<ServiceResponse<MessageModel>> Delete(int id);
    }
}
