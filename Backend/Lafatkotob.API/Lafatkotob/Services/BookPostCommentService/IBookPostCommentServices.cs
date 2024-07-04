using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.BookPostCommentService
{
    public interface IBookPostCommentServices
    {
        Task<ServiceResponse<BookPostCommentModel>>Post(BookPostCommentModel model);
        Task<BookPostCommentModel> GetById(int id);
        Task<List<BookPostCommentModel>> GetAll();
        Task<ServiceResponse<BookPostCommentModel>> Update(BookPostCommentModel model);
        Task<ServiceResponse<BookPostCommentModel>> Delete(int id);
    }
}
