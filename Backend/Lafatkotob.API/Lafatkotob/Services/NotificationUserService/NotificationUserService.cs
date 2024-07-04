using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.NotificationUserService
{
    public class NotificationUserService : INotificationUserService
    {
        private readonly ApplicationDbContext _context;

        public NotificationUserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<NotificationUserModel>> Delete(int id)
        {
            var response = new ServiceResponse<NotificationUserModel>();

            var NotificationUser = await _context.NotificationUsers.FindAsync(id);
            if (NotificationUser == null)
            {
                response.Success = false;
                response.Message = "NotificationUser not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.NotificationUsers.Remove(NotificationUser);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new NotificationUserModel
                        {
                            Id = NotificationUser.Id,
                            UserId = NotificationUser.UserId,
                            NotificationId = NotificationUser.NotificationId
                        };
                        
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete NotificationUser: {ex.Message}";
                    }
                }
            });

            return response;
        }
        private static string ConvertToFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            var baseUrl = "https://localhost:7139";
            return $"{baseUrl}{relativePath}";
        }
        public async Task<NotificationUserModel> GetById(int id)
        {
            var notificationUser = await _context.NotificationUsers
                .FirstOrDefaultAsync(nu => nu.Id == id);
            if (notificationUser == null) return null;

            return new NotificationUserModel
            {
                Id = notificationUser.Id,
                UserId = notificationUser.UserId,
                NotificationId = notificationUser.NotificationId
            };
        }
        public async Task<List<NotificationModel>> GetByUserId(string userid)
        {
           
            var notificationUser = await _context.NotificationUsers.Where(nu => nu.UserId == userid)
                .Include(no=>no.Notification)
                .OrderByDescending(c => c.Notification.DateSent)
                .Select(nu => new NotificationModel
                  {
                      Id = nu.Notification.Id,
                      UserId = nu.Notification.UserId,
                      Message = nu.Notification.Message,
                      ImgUrl = ConvertToFullUrl(nu.Notification.imgUrl),
                     IsRead = nu.Notification.IsRead,
                    DateSent = nu.Notification.DateSent,
                      UserName = nu.Notification.UserName,

                  })
               .ToListAsync();

            if (notificationUser == null) return null;

            return notificationUser;


        }
        public async Task<Int128> GetByUserIdFalse(string userid)
        {

            //var notificationUser = await _context.NotificationUsers.Where(nu => nu.UserId == userid )
            //    .Include(no => no.Notification)
            //    .OrderByDescending(c => c.Notification.DateSent)
            //    .Select(nu => new NotificationModel
            //    {
            //        Id = nu.Notification.Id,
            //        UserId = nu.Notification.UserId,
            //        Message = nu.Notification.Message,
            //        ImgUrl = ConvertToFullUrl(nu.Notification.imgUrl),
            //        IsRead = nu.Notification.IsRead,
            //        DateSent = nu.Notification.DateSent,
            //        UserName = nu.Notification.UserName,

            //    })
            //   .ToListAsync();
            var unreadNotificationCount = await _context.NotificationUsers
             .Where(nu => nu.UserId == userid && nu.Notification.IsRead == false)
             .CountAsync();
           

            return unreadNotificationCount;


        }

        public async Task<List<NotificationModel>> GetByUserIdFive(string userid)
        {

            var notificationUser = await _context.NotificationUsers.Where(nu => nu.UserId == userid)
                .Include(no => no.Notification)
                .OrderByDescending(c => c.Notification.DateSent)
                .Take(10)
                .Select(nu => new NotificationModel
                {
                    Id = nu.Notification.Id,
                    UserId = nu.Notification.UserId,
                    Message = nu.Notification.Message,
                    ImgUrl = ConvertToFullUrl(nu.Notification.imgUrl),
                    DateSent = nu.Notification.DateSent,
                    IsRead=nu.Notification.IsRead,
                    UserName = nu.Notification.UserName,

                })
               .ToListAsync();

            if (notificationUser == null) return null;

            return notificationUser;


        }

        public async Task<List<NotificationUserModel>> GetAll()
        {
            return await _context.NotificationUsers
                .Select(nu => new NotificationUserModel
                {
                    Id = nu.Id,
                    UserId = nu.UserId,
                    NotificationId = nu.NotificationId
                })
                .ToListAsync();
        }



        public async Task<ServiceResponse<NotificationUserModel>> Post(NotificationUserModel model)
        {
            var response = new ServiceResponse<NotificationUserModel>();

           
                    try
                    {

                var NotificationUser = new NotificationUser
                        {
                            UserId = model.UserId,
                            NotificationId = model.NotificationId
                        };
                       
                        _context.NotificationUsers.Add(NotificationUser);
                        await _context.SaveChangesAsync();
                    model.Id = NotificationUser.Id;

                response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        
                        response.Success = false;
                        response.Message = "Failed to create NotificationUser.";
                    }
            

            return response;
        }

        public async Task<ServiceResponse<NotificationUserModel>> Update(NotificationUserModel model)
        {
            var response = new ServiceResponse<NotificationUserModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var NotificationUser = await _context.NotificationUsers.FindAsync(model.Id);
            if (NotificationUser == null)
            {
                response.Success = false;
                response.Message = "NotificationUser not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        NotificationUser.NotificationId = model.NotificationId;
                        NotificationUser.UserId = model.UserId;

                        _context.NotificationUsers.Update(NotificationUser);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update NotificationUser: {ex.Message}";
                    }
                }
            });

            return response;
        }
    
    }
}
