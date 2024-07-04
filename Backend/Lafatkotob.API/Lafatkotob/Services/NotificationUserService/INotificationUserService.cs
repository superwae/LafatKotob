using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.NotificationUserService
{
    public interface INotificationUserService
    {
        Task<ServiceResponse<NotificationUserModel>> Post(NotificationUserModel model);
        Task<NotificationUserModel> GetById(int id);
        Task<List<NotificationUserModel>> GetAll();
        Task<ServiceResponse<NotificationUserModel>> Update(NotificationUserModel model);
        Task<ServiceResponse<NotificationUserModel>> Delete(int id);
        Task<List<NotificationModel>> GetByUserIdFive(string userid);
        Task<List<NotificationModel>> GetByUserId(string userid);
        Task<Int128> GetByUserIdFalse(string userid);
    }
}
