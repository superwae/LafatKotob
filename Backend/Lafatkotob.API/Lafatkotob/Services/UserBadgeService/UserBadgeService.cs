using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lafatkotob.Hubs;
using Microsoft.AspNetCore.SignalR;
using static System.Net.WebRequestMethods;
namespace Lafatkotob.Services.UserBadgeService
{
    public class UserBadgeService : IUserBadgeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserConnectionsService _userConnectionsService;
        private readonly IHubContext<ChatHub> _hubContext;
        public UserBadgeService(ApplicationDbContext context, IHubContext<ChatHub> hubContext,
            IUserConnectionsService userConnectionsService)
        {
            _context = context;
            _hubContext = hubContext;
            _userConnectionsService = userConnectionsService;
        }

        public async Task<ServiceResponse<UserBadgeModel>> Delete(int id)
        {
            var response = new ServiceResponse<UserBadgeModel>();

            var UserBadge = await _context.UserBadges.FindAsync(id);
            if (UserBadge == null)
            {
                response.Success = false;
                response.Message = "UserBadge not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.UserBadges.Remove(UserBadge);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new UserBadgeModel
                        {
                            Id = UserBadge.Id,
                            UserId = UserBadge.UserId,
                            BadgeId = UserBadge.BadgeId,
                            DateEarned = UserBadge.DateEarned
                        };

                        Notification notification = new Notification
                        {
                            UserId = UserBadge.UserId,
                            Message = $"You have lost a {UserBadge.Badge.BadgeName} badge",
                            DateSent = DateTime.Now,
                            IsRead = false,
                            imgUrl = "Lafatkotob.API//Lafatkotob//wwwroot//uploads//badge.jpg",


                        };
                        _context.Notifications.Add(notification);
                        await _context.SaveChangesAsync();

                        _context.NotificationUsers.Add(new NotificationUser
                        {
                            NotificationId = notification.Id,
                            UserId = UserBadge.UserId
                        });
                        await _context.SaveChangesAsync();


                        var Users = new List<string> { UserBadge.UserId };
                        foreach (var userId in Users)
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
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete UserBadge: {ex.Message}";
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
        public async Task<UserBadgeModel> GetById(int id)
        {
            var userBadge = await _context.UserBadges
                .FirstOrDefaultAsync(ub => ub.Id == id);
            if (userBadge == null) return null;

            return new UserBadgeModel
            {
                Id = userBadge.Id,
                UserId = userBadge.UserId,
                BadgeId = userBadge.BadgeId,
                DateEarned = userBadge.DateEarned
            };
        }

        public async Task<List<UserBadgeModel>> GetAll()
        {
            return await _context.UserBadges
                .Select(ub => new UserBadgeModel
                {
                    Id = ub.Id,
                    UserId = ub.UserId,
                    BadgeId = ub.BadgeId,
                    DateEarned = ub.DateEarned
                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<UserBadgeModel>> Post(UserBadgeModel model)
        {
            var response = new ServiceResponse<UserBadgeModel>();



            var UserBadge = new UserBadge
            {
                Id = model.Id,
                UserId = model.UserId,
                BadgeId = model.BadgeId,
                DateEarned = DateTime.Now
            };

            _context.UserBadges.Add(UserBadge);
            _context.SaveChanges();


            response.Success = true;
            response.Data = model;
            var badge = await _context.Badges.FindAsync(UserBadge.BadgeId);
            var user = await _context.Users.FindAsync(UserBadge.UserId);
            Notification notification = new Notification
            {
                UserId = UserBadge.UserId,
                Message = $"You have got a {badge.BadgeName} badge",
                DateSent = DateTime.Now,
                IsRead = false,
                UserName = user.UserName, 
                imgUrl = "/uploads/badge.jpg",

                
            };
            _context.Notifications.Add(notification);
            _context.SaveChanges();

            _context.NotificationUsers.Add(new NotificationUser
            {
                NotificationId = notification.Id,
                UserId = UserBadge.UserId
            });
            _context.SaveChanges();


            var Users = new List<string> { UserBadge.UserId };
            foreach (var userId in Users)
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



            return response;
        }

        public async Task<ServiceResponse<UserBadgeModel>> Update(UserBadgeModel model)
        {
            var response = new ServiceResponse<UserBadgeModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var UserBadge = await _context.UserBadges.FindAsync(model.Id);
            if (UserBadge == null)
            {
                response.Success = false;
                response.Message = "UserBadge not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        UserBadge.DateEarned = model.DateEarned;


                        _context.UserBadges.Update(UserBadge);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update UserBadge: {ex.Message}";
                    }
                }
            });

            return response;
        }

    }
}


