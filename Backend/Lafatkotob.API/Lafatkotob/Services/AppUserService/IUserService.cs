using Lafatkotob.ViewModel;
using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Identity;
using Lafatkotob.Services.TokenService;

namespace Lafatkotob.Services.AppUserService
{
    public interface IUserService
    {
        Task<ServiceResponse<AppUser>> RegisterUser(RegisterModel model,string rule, IFormFile imageFile);
        Task<ServiceResponse<UpdateUserModel>> UpdateUser(UpdateUserModel model, string userId, IFormFile imageFile);
        Task<IEnumerable<AppUserForAdminPageModel>> GetAllUsers(int pageNumber, int pageSize);
        Task<AppUserModel> GetUserById(string userId);
        Task<AppUserModel> GetUserByUserName(string userName);
        Task<ServiceResponse<UpdateUserModel>> DeleteUser(string userId);
        Task<ServiceResponse<string>> UpdateProfilePicture(string userId, IFormFile imageFile);
        Task<ServiceResponse<string>> UpdateBio(string userId, string bio);
        Task<ServiceResponse<string>> UpdateCity(string userId, string city);
        Task<LoginResultModel> LoginUser(LoginModel model);
        Task<ServiceResponse<AppUser>> SetHistoryId(string UserId,int HistoryId);
        Task<ServiceResponse<AppUser>> UpdateUserSetting(UpdateUserSettingModel model);
        Task<ServiceResponse<List<AppUserForAdminPageModel>>> UserSearch(string query);
        Task<ServiceResponse<string>> UpdateUserRole(string userId, string role);
        Task<ServiceResponse<bool>> ToggleDelete(string userId);
        

    }
}
