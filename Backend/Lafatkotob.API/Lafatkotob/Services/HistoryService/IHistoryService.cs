using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.HistoryService
{
    public interface IHistoryService
    {
        Task<ServiceResponse<int>> Post(string userId);
        Task<HistoryModel> GetById(int id);
        Task<List<HistoryModel>> GetAll();
        Task<ServiceResponse<HistoryModel>> Update(HistoryModel model);
        Task<ServiceResponse<HistoryModel>> Delete(int id);
    }
}
