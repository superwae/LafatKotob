using Lafatkotob.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.WishedBookService
{
    public interface IWishedBookService
    {
        Task<ServiceResponse<WishedBookModel>> Post(WishedBookModel model);
        Task<WishedBookModel> GetById(int id);
        Task<List<WishedBookModel>> GetAll();
        Task<ServiceResponse<WishedBookModel>> Update(WishedBookModel model);
        Task<ServiceResponse<WishedBookModel>> Delete(int id);
    }
}
