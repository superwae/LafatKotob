using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.BookGenreService
{
    public interface IBookGenreServices
    {
        Task<ServiceResponse<BookGenreModel>> Post(BookGenreModel model);
        Task<BookGenreModel> GetById(int id);
        Task<List<BookGenreModel>> GetAll();
        Task<ServiceResponse<BookGenreModel>> Update(BookGenreModel model);
        Task<ServiceResponse<BookGenreModel>> Delete(int id);
    }
}
