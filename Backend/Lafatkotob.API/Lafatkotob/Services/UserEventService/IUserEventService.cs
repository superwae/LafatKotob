using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.UserEventService
{
    public interface IUserEventService
    {
        Task<ServiceResponse<UserEventModel>> Post(UserEventModel model);
        Task<UserEventModel> GetById(int id);
        Task<List<UserEventModel>> GetAll();
        Task<ServiceResponse<UserEventModel>> Update(UserEventModel model);
        Task<ServiceResponse<UserEventModel>> Delete(int id);
        Task<ServiceResponse<UserEventModel>> DeleteUserEventByUserId(int EventId, string UserId);
        Task<UserEventModel> GetUserEvent(string UserId,int EventId);
    }
}
