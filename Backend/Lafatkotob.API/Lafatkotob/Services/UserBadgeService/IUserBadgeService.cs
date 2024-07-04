using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.UserBadgeService
{
    public interface IUserBadgeService
    {
        Task<ServiceResponse<UserBadgeModel>> Post(UserBadgeModel model);
        Task<UserBadgeModel> GetById(int id);
        Task<List<UserBadgeModel>> GetAll();
        Task<ServiceResponse<UserBadgeModel>> Update(UserBadgeModel model);
        Task<ServiceResponse<UserBadgeModel>> Delete(int id);
    }
}
