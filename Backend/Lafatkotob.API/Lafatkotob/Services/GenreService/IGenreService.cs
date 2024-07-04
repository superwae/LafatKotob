using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.GenreService
{
    public interface IGenreService
    {
        Task<ServiceResponse<GenreModel>> Post(GenreModel model);
        Task<GenreModel> GetById(int id);
        Task<List<GenreModel>> GetAll();
        Task<ServiceResponse<GenreModel>> Update(GenreModel model);
        Task<ServiceResponse<GenreModel>> Delete(int id);
    }
}
