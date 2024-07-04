using Lafatkotob.Entities;
using Lafatkotob.Services.EmailService;
using Lafatkotob.Services.TokenService;
using Lafatkotob.ViewModel;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Lafatkotob.Hubs;
namespace Lafatkotob.Services.AppUserService
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ITokenSerive _tokenService;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUserConnectionsService _userConnectionsService;
        public UserService(UserManager<AppUser> userManager, IHubContext<ChatHub> hubContex, IUserConnectionsService userConnectionsService, IEmailService emailService, ITokenSerive tokenService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _emailService = emailService;
            _tokenService = tokenService;
            _context = context;
            _userConnectionsService = userConnectionsService;
            _hubContext = hubContex;
        }

        public async Task<ServiceResponse<AppUser>> RegisterUser(RegisterModel model, string role, IFormFile imageFile)
        {

            var user = new AppUser
            {
                City = model.City,
                DateJoined = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                UserName = model.UserName,
                Email = model.Email,
                Name = model.Name,
                About = model.About,
                DTHDate = model.DTHDate,
                UpVotes = 0,
            };
            if (imageFile != null)
            {
                var imagePath = await SaveImageAsync(imageFile); 
                user.ProfilePicture = imagePath;
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return new ServiceResponse<AppUser>
                {
                    Success = false,
                    Message = "User registration failed.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        
            var roleResult = await _userManager.AddToRoleAsync(user, role);

            return new ServiceResponse<AppUser>
            {
                Success = true,
                Data = user,
                Message = "User registered successfully. Please check your email to confirm."
            };
        }

        public async Task<ServiceResponse<UpdateUserModel>> UpdateUser(UpdateUserModel model, string userId, IFormFile imageFile)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<UpdateUserModel> { Success = false, Message = "User not found." };
            }
            if (imageFile != null)
            {
                var imagePath = await SaveImageAsync(imageFile); 
                user.ProfilePicture = imagePath; 
            }
            if (model.Email != null && model.Email != user.Email)
            {
                var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                var changeEmailResult = await _userManager.ChangeEmailAsync(user, model.Email, emailToken);
                if (!changeEmailResult.Succeeded)
                {
                    return new ServiceResponse<UpdateUserModel>
                    {
                        Success = false,
                        Message = "Failed to update email.",
                        Errors = changeEmailResult.Errors.Select(e => e.Description).ToList()
                    };
                }
                user.EmailConfirmed = false; 
            }

            
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    return new ServiceResponse<UpdateUserModel>
                    {
                        Success = false,
                        Message = "New password and confirmation password do not match."
                    };
                }

                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!passwordCheck)
                {
                    return new ServiceResponse<UpdateUserModel>
                    {
                        Success = false,
                        Message = "Current password is incorrect."
                    };
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return new ServiceResponse<UpdateUserModel>
                    {
                        Success = false,
                        Message = "Failed to change password.",
                        Errors = changePasswordResult.Errors.Select(e => e.Description).ToList()
                    };
                }
            }

            user.UserName = model.UserName?? user.UserName;
            user.Name = model.Name ?? user.Name;
            user.City = model.City??user.City;
            user.About = model.About ?? user.About;
     
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ServiceResponse<UpdateUserModel>
                {
                    Success = false,
                    Message = "Failed to update user.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            return new ServiceResponse<UpdateUserModel> { Success = true ,Data=model};
        }


        public async Task<IEnumerable<AppUserForAdminPageModel>> GetAllUsers(int pageNumber, int pageSize)
        {
            var usersWithRoles = new List<AppUserForAdminPageModel>();

            foreach (var user in _userManager.Users.Skip((pageNumber - 1) * pageSize).Take(pageSize))
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userWithRoles = new AppUserForAdminPageModel
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Roles = roles.ToList(),
                    Email = user.Email,
                    DTHDate = user.DTHDate,
                    IsDeleted = user.IsDeleted,
                    City = user.City,
                    DateJoined = user.DateJoined,
                    LastLogin = user.LastLogin,
                    ProfilePicture = ConvertToFullUrl(user.ProfilePicture),
                    UpVotes = user.UpVotes ?? 0
                };
                usersWithRoles.Add(userWithRoles);
            }

            return usersWithRoles;
        }

        public async Task<AppUserModel> GetUserByUserName(string userName)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Name==userName);
            if (user == null)
            {
                return null;
            }

            var userModel = new AppUserModel
            {
                Id = user.Id,
                Name = user.Name,
                City = user.City,
                Email = user.Email,
                DateJoined = user.DateJoined,
                LastLogin = user.LastLogin,
                ProfilePicture = user.ProfilePicture,
                About = user.About,
                DTHDate = user.DTHDate,
                HistoryId = user.HistoryId,
                UpVotes = user.UpVotes ?? 0


            };
            userModel.ProfilePicture = ConvertToFullUrl(userModel.ProfilePicture);

            return userModel;
        }

        public async Task<AppUserModel> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var userModel = new AppUserModel
            {
                Id = user.Id,
                Name = user.Name,
                City = user.City,
                Email = user.Email,
                DateJoined = user.DateJoined,
                LastLogin = user.LastLogin,
                ProfilePicture = user.ProfilePicture,
                About = user.About,
                DTHDate = user.DTHDate,
                HistoryId = user.HistoryId,
                UpVotes = user.UpVotes ?? 0


            };
            userModel.ProfilePicture = ConvertToFullUrl(userModel.ProfilePicture);

            return userModel;
        }

        public async Task<ServiceResponse<UpdateUserModel>> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<UpdateUserModel> { Success = false, Message = "User not found." };
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return new ServiceResponse<UpdateUserModel>
                {
                    Success = false,
                    Message = "Failed to delete user.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            UpdateUserModel model = new UpdateUserModel
            {
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName,
                ProfilePictureUrl = user.ProfilePicture,
                About = user.About,
                City = user.City,
                CurrentPassword = null,
                NewPassword = null,
                ConfirmNewPassword = null
            };
            
            

            return new ServiceResponse<UpdateUserModel> { Success = true,Data= model };
        }

        public async Task<LoginResultModel> LoginUser(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return new LoginResultModel { Success = false, ErrorMessage = "Invalid username or password." };
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            var token = _tokenService.GenerateJwtToken(user.UserName, user.Id, roles.ToList());
            var response = new LoginResultModel
            {
                Success = true,
                Token = token,
                UserId = user.Id,
                UserName = user.UserName,
                ProfilePicture = ConvertToFullUrl(user.ProfilePicture),
                Role = new List<string> { },
                EmailConfirmed = user.EmailConfirmed,
                
            };
            foreach(var Role in roles)
            {

                    response.Role.Add(Role);
                
            }
            var now = DateTime.UtcNow;
            var lastLogin = user.LastLogin;

            // Check if this is the first login today
            if (lastLogin.Date < now.Date)
            {
                // Update the last login time
                user.LastLogin = now;
                await _userManager.UpdateAsync(user);

                // Check for events and send notifications
                await CheckAndSendEventNotifications(user);
            }

            return response;
        }
        private async Task CheckAndSendEventNotifications(AppUser user)
        {
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            var events= await _context.UserEvents
                .Include(ue => ue.Event)
                .Where(ue => ue.UserId == user.Id && ue.Event.DateScheduled.Date == tomorrow)
                .ToListAsync();
            for (int i = 0; i < events.Count; i++)
            {
                var event1 = events[i].Event;

                var notification = new Notification
                {

                    Message = $"You have an event scheduled for tomorrow.:{event1.Id}",
                    DateSent = DateTime.Now,
                    UserId = user.Id,
                    imgUrl= event1.ImagePath,
                    UserName=user.UserName,
                    IsRead= false,
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var notificatonUser = new NotificationUser
                {
                   
                    UserId = user.Id,
                    NotificationId = notification.Id,
                };
                _context.NotificationUsers.Add(notificatonUser);
                await _context.SaveChangesAsync();

                var UserIds=  new List<string> { user.Id };
                foreach (var userId in UserIds)
                {
                    var connections = _userConnectionsService.GetUserConnections(userId);

                    foreach (var connection in connections)
                    {
                        await _hubContext.Clients.Client(connection).SendAsync("NotificationModel", new
                        {
                            id = notification.Id,
                            message = notification.Message,
                            dateSent = notification.DateSent,
                            isRead = notification.IsRead,
                            userId = notification.UserId,
                            userName = notification.UserName,
                            imgUrl = ConvertToFullUrl(notification.imgUrl)
                        });
                    }
                }
            }
        }
        public async Task<ServiceResponse<string>> UpdateProfilePicture(string userId, IFormFile imageFile)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<string> { Success = false, Message = "User not found." };
            }

            if (imageFile != null)
            {
                var imagePath = await SaveImageAsync(imageFile);
                user.ProfilePicture = imagePath;
            }

            // Save the changes to the database
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var response = new ServiceResponse<string> { 
                    Success = true,
                    Message = "Profile picture updated successfully.",
                    Data = user.ProfilePicture
                };
                return response;
            }
            else
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Failed to update profile picture.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }

        public async Task<ServiceResponse<string>> UpdateBio(string userId, string bio)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<string> { Success = false, Message = "User not found." };
            }

            user.About = bio;

            // Save the changes to the database
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new ServiceResponse<string> { Success = true, Message = "Bio updated successfully." ,Data=bio};
            }
            else
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Failed to update bio.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }
        public async Task<ServiceResponse<string>> UpdateCity(string Id, string city)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return new ServiceResponse<string> { Success = false, Message = "User not found." };
            }

            user.City = city;

            // Save the changes to the database
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new ServiceResponse<string> { Success = true, Message = "city updated successfully.", Data = city };
            }
            else
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Failed to update city.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("The file is empty or null.", nameof(imageFile));
            }

            // Ensure the uploads directory exists
            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Generate a unique filename for the image to avoid name conflicts
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            // Save the file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            var imageUrl = $"/uploads/{fileName}";
            return imageUrl;
        }
        private string ConvertToFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            // Use your API's base URL here
            var baseUrl = "https://localhost:7139";
            return $"{baseUrl}{relativePath}";
        }

        public async Task<ServiceResponse<AppUser>> SetHistoryId(string UserId, int HistoryId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user == null)
            {
                return new ServiceResponse<AppUser> { Success = false, Message = "User not found." };
            }
            user.HistoryId = HistoryId;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ServiceResponse<AppUser>
                {
                    Success = false,
                    Message = "Failed to update user.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            return new ServiceResponse<AppUser> { Success = true, Data = user };
        }

        public async Task<ServiceResponse<AppUser>> UpdateUserSetting(UpdateUserSettingModel model)

        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return new ServiceResponse<AppUser> { Success = false, Message = "User not found." };
            }
      
            if (model.Email != null && model.Email != user.Email)
            {
                var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                var changeEmailResult = await _userManager.ChangeEmailAsync(user, model.Email, emailToken);
                if (!changeEmailResult.Succeeded)
                {
                    return new ServiceResponse<AppUser>
                    {
                        Success = false,
                        Message = "Failed to update email.",
                        Errors = changeEmailResult.Errors.Select(e => e.Description).ToList()
                    };
                }
                user.EmailConfirmed = false;
            }

            user.UserName = model.UserName ?? user.UserName;
            user.Name = model.UserName ?? user.Name;
            user.City = model.City ?? user.City;
            user.About = model.About ?? user.About;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ServiceResponse<AppUser>
                {
                    Success = false,
                    Message = "Failed to update user.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
   
            return new ServiceResponse<AppUser> { Success = true, Data = user };
        }

        public async Task<ServiceResponse<List<AppUserForAdminPageModel>>> UserSearch(string query)
        {
            var response = new ServiceResponse<List<AppUserForAdminPageModel>>();
            query = query.ToLower().Trim();


            var usersWithRoles = new List<AppUserForAdminPageModel>();

            foreach (var user in _userManager.Users.Where(b => b.Name.ToLower().Contains(query) || b.Email.ToLower().Contains(query))
)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userWithRoles = new AppUserForAdminPageModel
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Roles = roles.ToList(),
                    Email = user.Email,
                    DTHDate = user.DTHDate,
                    IsDeleted = user.IsDeleted,
                    City = user.City,
                    DateJoined = user.DateJoined,
                    LastLogin = user.LastLogin,
                    ProfilePicture = ConvertToFullUrl(user.ProfilePicture),



                };
                usersWithRoles.Add(userWithRoles);
            }

            if (usersWithRoles.Count > 0)
            {
                response.Success = true;
                response.Data = usersWithRoles;

                return response;
            }
            response.Success = false;
            response.Message = "No matching names found";

            return response;

        }

        public async Task<ServiceResponse<bool>> ToggleDelete(string userId)
        {
            ServiceResponse<bool> response = new ServiceResponse<bool>();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<bool> { Success = false, Message = "User not found." };
            }
            user.IsDeleted = !user.IsDeleted;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                // Handle update failure
                return new ServiceResponse<bool> { Success = false, Message = "Failed to update user." };
            }
            
            
            response.Success = true;
            response.Data = user.IsDeleted;
            response.Message = user.IsDeleted ? "User deleted successfully." : "User restored successfully.";
            return response;
        }


        public async Task<ServiceResponse<string>> UpdateUserRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<string> { Success = false, Message = "User not found." };
            }

            // Remove existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            try
            {
                // Add the new role
                var result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    return new ServiceResponse<string> { Success = true, Message = "User role updated successfully.", Data = role };
                }
                else
                {
                    return new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "Failed to update user role.",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }
            }
            catch (Exception e)
            {
                return new ServiceResponse<string> { Success = false, Message = "Failed to update user role.", Errors = new List<string> { e.Message } };
            }
            
        }

    }
}
