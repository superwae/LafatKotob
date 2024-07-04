using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.BooksInWishlistsService
{
    public interface IBooksInWishlistsService
    {
        Task<ServiceResponse<BookInWishlistsModel>> Post(BookInWishlistsModel model);
        Task<BookInWishlistsModel> GetById(int id);
        Task<List<BookInWishlistsModel>> GetAll();
        Task<ServiceResponse<BookInWishlistsModel>> Update(BookInWishlistsModel model);
        Task<ServiceResponse<BookInWishlistsModel>> Delete(int id);
        Task<ServiceResponse<BookInWishlistsModel>> PostAll(BookInWishlistsModel model, string UserId);
    }
}
