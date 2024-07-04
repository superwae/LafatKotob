using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.BookPostLikeServices
{
    public interface IBookPostLikeService
    {
        Task<ServiceResponse<AddBookPostLikeModel>> Post(AddBookPostLikeModel model);
        Task<BookPostLikeModel> GetById(AddBookPostLikeModel model);
        Task<List<BookPostLikeModel>> GetAll();
        Task<ServiceResponse<BookPostLikeModel>> Update(BookPostLikeModel model);
        Task<ServiceResponse<BookPostLikeModel>> Delete(AddBookPostLikeModel model);
        Task<ServiceResponse<Dictionary<int, bool>>> CheckBulkLikes(string userId, List<int> bookIds);
    }
}
