using Azure;
using Lafatkotob.Entities;
using Lafatkotob.Services.EventService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lafatkotob.Hubs;
using Microsoft.AspNetCore.Identity;
namespace Lafatkotob.Services.EventService
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserConnectionsService _userConnectionsService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UserManager<AppUser> _userManager;

        public EventService(ApplicationDbContext context, IHubContext<ChatHub> hubContext,
            IUserConnectionsService userConnectionsService, UserManager<AppUser> userManager )
        {
            _context = context;
            _hubContext = hubContext;
            _userConnectionsService = userConnectionsService;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<EventModel>> Post(EventModel model, IFormFile imageFile)
        {
            var response = new ServiceResponse<EventModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (imageFile != null && imageFile.Length > 0)
                        {
                            model.ImagePath = await SaveImageAsync(imageFile);

                        }
                        var Event = new Event
                        {
                            EventName = model.EventName,
                            Description = model.Description,
                            DateScheduled = model.DateScheduled,
                            ImagePath = model.ImagePath,
                            Location = model.Location,
                            SubLocation=model.SubLocation,
                            HostUserId = model.HostUserId,
                            attendances = 0
                        };
                       
                        _context.Events.Add(Event);
                        await _context.SaveChangesAsync();
                        model.ImagePath = ConvertToFullUrl(model.ImagePath);
                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create Event.";
                    }
                }
            });

            return response;
        }
        public async Task<AppUserModel> GetUserByEvent(int eventId)
        {
            //from events get the event that have tha same id 
            var eventEntity = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId);
            //from user get the user that have the same id as the hostUserId
            var userEntity = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == eventEntity.HostUserId);
            //return the user
            var user = new AppUserModel
            {
                Id = userEntity.Id,
                IsDeleted = userEntity.IsDeleted,
                Name = userEntity.Name,
                City = userEntity.City,
                Email = userEntity.Email,
                DateJoined = userEntity.DateJoined,
                LastLogin = userEntity.LastLogin,
                ProfilePicture =ConvertToFullUrl(userEntity.ProfilePicture),
                About = userEntity.About,
                DTHDate = userEntity.DTHDate,
                HistoryId = userEntity.HistoryId,
                UpVotes = userEntity.UpVotes,
            };



            return user;

        }
        public async Task<EventModel> GetById(int id)
        {
            var eventEntity = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            if (eventEntity == null) return null;

            return new EventModel
            {
                Id = eventEntity.Id,
                EventName = eventEntity.EventName,
                Description = eventEntity.Description,
                DateScheduled = eventEntity.DateScheduled,
                Location = eventEntity.Location,
                SubLocation=eventEntity.SubLocation,
                HostUserId = eventEntity.HostUserId,
                ImagePath = ConvertToFullUrl(eventEntity.ImagePath),
                attendances = eventEntity.attendances
            };
        }

        public async Task<ServiceResponse<List<EventModel>>> GetEventsByUserId(string userId)
        {
            var reposnse = new ServiceResponse<List<EventModel>>();
            var eventModels = await _context.UserEvents
                .Where(ue => ue.UserId == userId)  
                .Select(ue => new EventModel
                {
                    Id = ue.Event.Id,
                    EventName = ue.Event.EventName,
                    Description = ue.Event.Description,
                    DateScheduled = ue.Event.DateScheduled,
                    Location = ue.Event.Location,
                    SubLocation = ue.Event.SubLocation,
                    HostUserId = ue.Event.HostUserId,
                    ImagePath = ConvertToFullUrl(ue.Event.ImagePath),
                    attendances = ue.Event.attendances

                })
                .ToListAsync();
            if(eventModels == null || !eventModels.Any())
            {
                reposnse.Success = false;
                reposnse.Message = "No events found for the given user.";
                return reposnse;
            }
            reposnse.Success = true;
            reposnse.Data = eventModels;
            return reposnse;
        }
        public async Task<List<EventModel>> GetEventsByHostId(string userId)
        {
            var eventModels = await _context.Events
                .Where(e => e.HostUserId == userId)
                .Select(o => new EventModel
                {
                    Id = o.Id,
                    EventName = o.EventName,
                    Description = o.Description,
                    DateScheduled = o.DateScheduled,
                    Location = o.Location,
                    SubLocation = o.SubLocation,
                    HostUserId = o.HostUserId,
                    ImagePath = ConvertToFullUrl(o.ImagePath),
                    attendances = o.attendances

                })
                .ToListAsync();

            return eventModels;
        }

        public async Task<List<EventModel>> GetAll(int pageNumber, int pageSize)
        {
            return await _context.Events
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(b => b.DateScheduled)
                .Select(e => new EventModel
                {
                    Id = e.Id,
                    EventName = e.EventName,
                    Description = e.Description,
                    DateScheduled = e.DateScheduled,
                    Location = e.Location,
                    SubLocation=e.SubLocation,
                    HostUserId = e.HostUserId,
                    ImagePath = ConvertToFullUrl(e.ImagePath),
                    attendances = e.attendances

                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<EventModel>> Update(int eventId, EventModel model, IFormFile imageFile)
        {
            var response = new ServiceResponse<EventModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var Event = await _context.Events.FindAsync(eventId);
            if (Event == null)
            {
                response.Success = false;
                response.Message = "Event not found.";
                return response;
            }
            if (imageFile != null && imageFile.Length > 0)
            {
                var imagePath = await SaveImageAsync(imageFile); 
                Event.ImagePath = imagePath; 
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        Event.Location = model.Location;
                        Event.SubLocation = model.SubLocation;
                        Event.Description = model.Description;
                        Event.DateScheduled = model.DateScheduled;
                        Event.EventName = model.EventName;
                        Event.HostUserId = model.HostUserId;
                        Event.attendances = model.attendances;


                        _context.Events.Update(Event);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update Event: {ex.Message}";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<EventModel>> DeleteAllRelationAndEvent(int id)
        {
            var response = new ServiceResponse<EventModel>();

            var Event = await _context.Events.FindAsync(id);
            if (Event == null)
            {
                response.Success = false;
                response.Message = "Event not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Delete related EventUser records
                        var eventUsers = _context.UserEvents.Where(eu => eu.EventId == id);
                        var notification = new Notification
                        {
                            UserId = Event.HostUserId,
                            Message = $"Event {Event.EventName} has been deleted.",
                            DateSent = DateTime.Now,
                            IsRead = false,
                            imgUrl = Event.ImagePath,
                            UserName= "null",
                        };
                        _context.Notifications.Add(notification);
                        _context.SaveChanges();
                       List<string> Users = new List<string>();
                        for (int i=0;i< eventUsers.Count(); i++)
                        {
                            var notificationUser = new NotificationUser
                            {
                                NotificationId = notification.Id,
                                UserId = eventUsers.ToList()[i].UserId,
                            };
                            Users.Add(eventUsers.ToList()[i].UserId);
                            _context.NotificationUsers.Add(notificationUser);
                            _context.SaveChanges();
                        }
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
                       

                        _context.UserEvents.RemoveRange(eventUsers);
                        // Delete the Event itself
                        _context.Events.Remove(Event);
                        _context.SaveChanges();
                        await transaction.CommitAsync();
                        response.Success = true;
                        response.Data = new EventModel
                        {
                            Id = Event.Id,
                            EventName = Event.EventName,
                            Description = Event.Description,
                            DateScheduled = Event.DateScheduled,
                            Location = Event.Location,
                            HostUserId = Event.HostUserId,
                            attendances = Event.attendances
                        };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete Event: {ex.Message}";
                    }
                }
            });

            return response;
        }
        public async Task<ServiceResponse<EventModel>> Delete(int id)
        {
            var response = new ServiceResponse<EventModel>();

            var Event = await _context.Events.FindAsync(id);
            if (Event == null)
            {
                response.Success = false;
                response.Message = "Event not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Events.Remove(Event);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new EventModel
                        {
                            Id = Event.Id,
                            EventName = Event.EventName,
                            Description = Event.Description,
                            DateScheduled = Event.DateScheduled,
                            Location = Event.Location,
                            SubLocation=Event.SubLocation,
                            HostUserId = Event.HostUserId,
                            attendances = Event.attendances
                        };
                        
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete Event: {ex.Message}";
                    }
                }
            });

            return response;
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

        public async Task<ServiceResponse<List<EventModel>>> GetEventsByCity(string city, int pageNumber, int pageSize)
        {
            var response = new ServiceResponse<List<EventModel>>();
            try
            {
                var eventsQuery = _context.Events
                    .Where(e => EF.Functions.Like(e.Location, $"%{city}%"));

                response.TotalItems = await eventsQuery.CountAsync(); 
                var events = await eventsQuery
                    .OrderBy(e => e.DateScheduled) 
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new EventModel
                    {
                        Id = e.Id,
                        EventName = e.EventName,
                        Description = e.Description,
                        DateScheduled = e.DateScheduled,
                        Location = e.Location,
                        SubLocation=e.SubLocation,
                        HostUserId = e.HostUserId,
                        ImagePath = ConvertToFullUrl(e.ImagePath),
                        attendances = e.attendances
                    })
                    .ToListAsync();

                if (!events.Any())
                {
                    response.Success = false;
                    response.Message = "No events found for the given city.";
                    return response;
                }

                response.Success = true;
                response.Data = events;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred while retrieving events: {ex.Message}";
            }

            return response;
        }



        private static string ConvertToFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            var baseUrl = "https://localhost:7139";
            return $"{baseUrl}{relativePath}";
        }

        public async Task<ServiceResponse<List<EventModel>>> GetBooksByUserName(string UserId, int pageNumber, int pageSize, bool GetRegisterd)
        {
            var response = new ServiceResponse<List<EventModel>>();

            if (GetRegisterd)
            {
                var eventModels = await _context.UserEvents
                    .Where(ue => ue.UserId == UserId)
                    .OrderBy(ue => ue.Event.DateScheduled)
                    .Select(ue => new EventModel
                    {
                        Id = ue.Event.Id,
                        EventName = ue.Event.EventName,
                        Description = ue.Event.Description,
                        DateScheduled = ue.Event.DateScheduled,
                        Location = ue.Event.Location,
                        SubLocation=ue.Event.SubLocation,
                        HostUserId = ue.Event.HostUserId,
                        ImagePath = ConvertToFullUrl(ue.Event.ImagePath),
                        attendances = ue.Event.attendances

                    })
                    .ToListAsync();
                if (eventModels == null || !eventModels.Any())
                {
                    response.Success = false;
                    response.Message = "No events found for the given user.";
                    return response;
                }
                response.Success = true;
                response.Data = eventModels;
                return response;
            }
            else
            {
                var eventModels = await _context.Events
                    .Where(e => e.HostUserId == UserId)
                    .OrderBy(e => e.DateScheduled)
                    .Select(e => new EventModel
                    {
                        Id = e.Id,
                        EventName = e.EventName,
                        Description = e.Description,
                        DateScheduled = e.DateScheduled,
                        Location = e.Location,
                        SubLocation=e.SubLocation,
                        HostUserId = e.HostUserId,
                        ImagePath = ConvertToFullUrl(e.ImagePath),
                        attendances = e.attendances

                    })
                    .ToListAsync();
                if (eventModels == null || !eventModels.Any())
                {
                    response.Success = false;
                    response.Message = "No events found for the given user.";
                    return response;
                }
                response.Success = true;
                response.Data = eventModels;
                return response;
            }
        }

        public async Task<ServiceResponse<List<Event>>> SearchEvents(string query, int pageNumber, int pageSize)
        {
            var response = new ServiceResponse<List<Event>>();
            query = query.ToLower().Trim();

            var eventsQuery = _context.Events
                .Where(e => e.EventName.ToLower().Contains(query) || e.HostUser.Name.ToLower().Contains(query));

            response.TotalItems = await eventsQuery.CountAsync(); 
            var events = await eventsQuery
                .OrderBy(e => e.DateScheduled) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            events.ForEach(e =>
            {
                e.ImagePath = ConvertToFullUrl(e.ImagePath);
            });

            response.Success = true;
            response.Data = events;
            if (!events.Any())
            {
                response.Message = "No events found matching the search criteria.";
                response.Success = false;
            }
            return response;
        }

    }
}
