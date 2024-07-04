using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.NotificationService
{
    public interface INotificationService
    {
        Task<ServiceResponse<NotificationModel>> Post(NotificationModel model);
        Task<NotificationModel> GetById(int id);
        Task<List<NotificationModel>> GetAll();
        Task<ServiceResponse<NotificationModel>> Update(NotificationModel model);
        Task<ServiceResponse<NotificationModel>> Delete(int id);
       
    }
}
