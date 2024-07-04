using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.UserReviewService
{
    public interface IUserReviewService
    {
        Task<ServiceResponse<UserReviewModel>> Post(UserReviewModel model);
        Task<UserReviewModel> GetById(int id);
        Task<List<UserReviewModel>> GetAll();
        Task<ServiceResponse<UserReviewModel>> Update(UserReviewModel model);
        Task<ServiceResponse<UserReviewModel>> Delete(int id);
    }
}
