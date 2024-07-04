using Lafatkotob.Entities;
using Lafatkotob.ViewModels;

namespace Lafatkotob.Services.EventService
{
    public interface IEventService
    {
        Task<ServiceResponse<EventModel>> Post(EventModel model, IFormFile imageFile);
        Task<ServiceResponse<EventModel>> Update(int eventId, EventModel model, IFormFile imageFile);
        Task<EventModel> GetById(int id);
        Task<ServiceResponse<List<EventModel>>> GetEventsByUserId(string userId);
        Task<List<EventModel>> GetAll(int pageNumber, int pageSize);
        Task<ServiceResponse<EventModel>> Delete(int id);
        Task<List<EventModel>> GetEventsByHostId(string userId);
        Task<AppUserModel> GetUserByEvent(int eventId );
        Task<ServiceResponse<List<EventModel>>> GetEventsByCity(string city, int pageNumber, int pageSize);
        Task<ServiceResponse<EventModel>> DeleteAllRelationAndEvent(int id);
        Task<ServiceResponse<List<EventModel>>> GetBooksByUserName(string UserId, int pageNumber, int pageSize,bool GetRegisterd);
        Task<ServiceResponse<List<Event>>> SearchEvents(string query, int pageNumber, int pageSize);


    }
}
