using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.UserLikeService
{
    public interface IUserLikeService
    {
        Task<ServiceResponse<UserLikeModel>> Post(UserLikeModel model);
        Task<UserLikeModel> GetById(int id);
        Task<List<UserLikeModel>> GetAll();
        Task<ServiceResponse<UserLikeModel>> Update(UserLikeModel model);
        Task<ServiceResponse<UserLikeModel>> Delete(int id);
    }
}
