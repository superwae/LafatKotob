using Lafatkotob.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.WishListService
{
    public interface IWishListService
    {
        Task<ServiceResponse<WishlistModel>> Post(WishlistModel model);
        Task<ServiceResponse<WishlistModel>> GetById(int id);
        Task<ServiceResponse<List<WishlistModel>>> GetAll();
        Task<ServiceResponse<WishlistModel>> Update(WishlistModel model);
        Task<ServiceResponse<WishlistModel>> Delete(int id);
        Task<ServiceResponse<List<BookInWishlistsModel>>> GetByUserId(string userId);
    }
}
