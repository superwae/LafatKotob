using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.BadgeService
{
    public interface IBadgeService
    {
        Task<ServiceResponse<BadgeModel>> Post(BadgeModel model);
        Task<BadgeModel> GetById(int id);
        Task<List<BadgeModel>> GetAll();
        Task<ServiceResponse<BadgeModel>> Update(BadgeModel model);
        Task<ServiceResponse<BadgeModel>> Delete(int id);
        Task<ServiceResponse<List<BadgeModel>>> GetAllBadgesByUser(string userId);
    }
}
