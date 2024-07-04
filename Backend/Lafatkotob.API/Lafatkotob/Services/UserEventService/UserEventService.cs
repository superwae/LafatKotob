using Lafatkotob.Entities;
using Lafatkotob.Hubs;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lafatkotob.Hubs;
using Microsoft.AspNetCore.SignalR;
using Lafatkotob.Services.UserBadgeService;

namespace Lafatkotob.Services.UserEventService
{
    public class UserEventService : IUserEventService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserConnectionsService _userConnectionsService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUserBadgeService _userBadge;
       public UserEventService(ApplicationDbContext context, UserManager<AppUser> userManager, IHubContext<ChatHub> hubContext,
            IUserConnectionsService userConnectionsService, IUserBadgeService userBadge)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _userConnectionsService = userConnectionsService;
            _userBadge = userBadge;
        }

        public async Task<ServiceResponse<UserEventModel>> Delete(int id)
        {
            var response = new ServiceResponse<UserEventModel>();

            var UserEvent = await _context.UserEvents.FindAsync(id);
            if (UserEvent == null)
            {
                response.Success = false;
                response.Message = "UserEvent not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.UserEvents.Remove(UserEvent);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new UserEventModel
                        {
                            Id = UserEvent.Id,
                            UserId = UserEvent.UserId,
                            EventId = UserEvent.EventId
                        };

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete UserEvent: {ex.Message}";
                    }
                }
            });

            return response;
        }
        public async Task<ServiceResponse<UserEventModel>> DeleteUserEventByUserId(int EventId, string UserId)
        {
            var response = new ServiceResponse<UserEventModel>();

            var userEvent = await _context.UserEvents.FirstOrDefaultAsync(ue => ue.EventId == EventId && ue.UserId == UserId);

            if (userEvent == null)
            {
                response.Success = false;
                response.Message = "UserEvent not found.";
                return response;
            }



            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
                        var allUser = _userManager.FindByIdAsync(user.Id);
                        var eventM = await _context.Events.FirstOrDefaultAsync(e => e.Id == EventId);
                        if (eventM == null)
                        {
                            response.Success = false;
                            response.Message = "Event not found.";
                            return;
                        }
                        eventM.attendances -= 1;
                        if(eventM.attendances < 0)
                        {
                            eventM.attendances = 0;
                        }
                        var notifiations = new Notification
                        {
                            UserId = userEvent.UserId,
                            Message = $"{user.UserName} canceled the event attendance registration for {eventM.EventName}:{eventM.Id}",
                            DateSent = DateTime.Now,
                            IsRead = false,
                            imgUrl = allUser.Result.ProfilePicture,
                            UserName = user.UserName,

                        };
                        _context.Notifications.Add(notifiations);
                        _context.SaveChanges();
                        var notificationUser = new NotificationUser
                        {
                            NotificationId = notifiations.Id,
                            UserId = eventM.HostUserId
                        };
                        _context.NotificationUsers.Add(notificationUser);
                        _context.SaveChanges();
                        List<string> Users = new List<string>();
                        Users.Add(eventM.HostUserId);
                       
                        foreach (var userId in Users)
                        {
                            var connections = _userConnectionsService.GetUserConnections(userId);

                            foreach (var connection in connections)
                            {
                                await _hubContext.Clients.Client(connection).SendAsync("NotificationModel", new
                                {
                                    id = notifiations.Id,
                                    message = notifiations.Message,
                                    dateSent = notifiations.DateSent,
                                    isRead = notifiations.IsRead,
                                    userId = notifiations.UserId,
                                    userName = notifiations.UserName,
                                    imgUrl = ConvertToFullUrl(notifiations.imgUrl)
                                });
                            }
                        }

                        _context.UserEvents.Remove(userEvent);
                        _context.SaveChanges();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new UserEventModel
                        {
                            Id = userEvent.Id,
                            UserId = userEvent.UserId,
                            EventId = userEvent.EventId
                        };

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete UserEvent: {ex.Message}";
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
        public async Task<UserEventModel> GetById(int id)
        {
            var userEvent = await _context.UserEvents
                .FirstOrDefaultAsync(ue => ue.Id == id);
            if (userEvent == null) return null;

            return new UserEventModel
            {
                Id = userEvent.Id,
                UserId = userEvent.UserId,
                EventId = userEvent.EventId
            };
        }

        public async Task<List<UserEventModel>> GetAll()
        {
            return await _context.UserEvents
                .Select(ue => new UserEventModel
                {
                    Id = ue.Id,
                    UserId = ue.UserId,
                    EventId = ue.EventId
                })
                .ToListAsync();
        }

        public async Task<UserEventModel> GetUserEvent(string UserId, int EventId)
        {
            var userEvent = await _context.UserEvents
               .FirstOrDefaultAsync(ue => ue.UserId == UserId && ue.EventId == EventId);
            if (userEvent == null) return null;

            return new UserEventModel
            {
                Id = userEvent.Id,
                UserId = userEvent.UserId,
                EventId = userEvent.EventId
            };

        }

        public async Task<ServiceResponse<UserEventModel>> Post(UserEventModel model)
        {
            var response = new ServiceResponse<UserEventModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Check if the UserEvent already exists
                        var existingUserEvent = await _context.UserEvents.FirstOrDefaultAsync(ue => ue.UserId == model.UserId && ue.EventId == model.EventId);

                        if (existingUserEvent != null)
                        {
                            // If the UserEvent already exists, return an error response
                            response.Success = false;
                            response.Message = "UserEvent already exists.";
                        }
                        else
                        {
                            // If the UserEvent does not exist, create and save it
                            var newUserEvent = new UserEvent
                            {
                                UserId = model.UserId,
                                EventId = model.EventId
                            };
                            _context.UserEvents.Add(newUserEvent);
                            await _context.SaveChangesAsync();
                            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
                            var userAll = _userManager.FindByIdAsync(user.Id);
                            var eventM = await _context.Events.FirstOrDefaultAsync(e => e.Id == model.EventId);
                            if(eventM == null)
                            {
                                response.Success = false;
                                response.Message = "Event not found.";
                                return;
                            }
                            eventM.attendances += 1;
                            var notifiction = new Notification
                            {
                                UserId = user.Id,
                                Message = $"{user.UserName} register to attend the {eventM.EventName} event :{eventM.Id}",
                                DateSent = DateTime.Now,
                                IsRead = false,
                                imgUrl = userAll.Result.ProfilePicture,
                                UserName = user.UserName,

                            };
                            _context.Notifications.Add(notifiction);
                            await _context.SaveChangesAsync();
                            var notificationUser = new NotificationUser
                            {
                                Notification = notifiction,
                                UserId = eventM.HostUserId
                            };
                            _context.NotificationUsers.Add(notificationUser);
                            await _context.SaveChangesAsync();

                            var users = new List<string> { eventM.HostUserId };
                            //await _hubContext.Clients.Users(users).SendAsync("NotificationModel", notification);
                            
                            foreach (var userId in users)
                            {
                                var connections = _userConnectionsService.GetUserConnections(userId);

                                foreach (var connection in connections)
                                {
                                    await _hubContext.Clients.Client(connection).SendAsync("NotificationModel", new
                                    {
                                        id = notifiction.Id,
                                        message = notifiction.Message,
                                        dateSent = notifiction.DateSent,
                                        isRead = notifiction.IsRead,
                                        userId = notifiction.UserId,
                                        userName = notifiction.UserName,
                                        imgUrl = ConvertToFullUrl(notifiction.imgUrl)
                                    });
                                }
                            }
                            var participleCount = await _context.UserEvents
                            .Where(ue => ue.UserId == model.UserId)
                            .CountAsync();

                            if (participleCount >= 10)
                            {
                                var badge = await _context.Badges.Where(b => b.BadgeName == "ParticipationGold")
                                .FirstOrDefaultAsync();
                                var userbadge = await _context.UserBadges.Where(us => us.BadgeId == badge.Id && us.UserId == model.UserId).FirstOrDefaultAsync();
                                if (userbadge == null)
                                {
                                    var userBadge = new UserBadgeModel
                                    {
                                        BadgeId = badge.Id,
                                        UserId = model.UserId,
                                        DateEarned = DateTime.Now

                                    };
                                    _userBadge.Post(userBadge);
                                    await _context.SaveChangesAsync();
                                }

                            }
                            else if (participleCount >= 3)
                            {
                                var badge = await _context.Badges.Where(b => b.BadgeName == "ParticipationSilver")
                             .FirstOrDefaultAsync();

                                var userbadge = await _context.UserBadges.Where(us => us.BadgeId == badge.Id && us.UserId == model.UserId).FirstOrDefaultAsync();
                                if (userbadge == null)
                                {

                                    var userBadge = new UserBadgeModel
                                    {
                                        BadgeId = badge.Id,
                                        UserId = model.UserId,
                                        DateEarned = DateTime.Now

                                    };

                                    _userBadge.Post(userBadge);
                                    await _context.SaveChangesAsync();
                                }
                            }
                            else if (participleCount >=1)
                            {
                                var badgee = await _context.Badges.Where(b => b.BadgeName == "ParticipationBronze")
                                
                                .FirstOrDefaultAsync();

                                var userbadgee = await _context.UserBadges.Where(us => us.BadgeId == badgee.Id && us.UserId == model.UserId)
                                 
                                .FirstOrDefaultAsync();

                                if (userbadgee == null)
                                {
                                    var userBadge = new UserBadgeModel
                                    {
                                        BadgeId = badgee.Id,
                                        UserId = model.UserId,
                                        DateEarned = DateTime.Now

                                    };
                                    _userBadge.Post(userBadge);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }


                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }

                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create UserEvent.";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<UserEventModel>> Update(UserEventModel model)
        {
            var response = new ServiceResponse<UserEventModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var UserEvent = await _context.UserEvents.FindAsync(model.Id);
            if (UserEvent == null)
            {
                response.Success = false;
                response.Message = "UserEvent not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        UserEvent.EventId = model.EventId;
                        UserEvent.UserId = model.UserId;


                        _context.UserEvents.Update(UserEvent);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update UserEvent: {ex.Message}";
                    }
                }
            });

            return response;
        }
    }
}