using Azure;
using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Lafatkotob.Services.NotificationService;
using Lafatkotob.Services.NotificationUserService;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Lafatkotob.Hubs;
using System.Collections.Specialized;
using Microsoft.Extensions.Logging;

namespace Lafatkotob.Services.BookPostLikeServices
{

    public class BookPostLikeService : IBookPostLikeService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly INotificationUserService _notificationUserService;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppUser appUser1;
        private readonly IUserConnectionsService _userConnectionsService;
        private readonly IHubContext<ChatHub> _hubContext;
        public BookPostLikeService(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            INotificationService notificationService,
            INotificationUserService notificationUserService,
            IHubContext<ChatHub> hubContext,
            IUserConnectionsService userConnectionsService
        )
        {
            _hubContext = hubContext;
            _userConnectionsService = userConnectionsService;
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _notificationUserService = notificationUserService;

        }
        public async Task<ServiceResponse<AddBookPostLikeModel>> Post(AddBookPostLikeModel model)
        {
            var response = new ServiceResponse<AddBookPostLikeModel>();
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            // Find the user and get their profile image URL
            var appUser = await _userManager.FindByIdAsync(model.UserId);
            var profileImageUrl = appUser?.ProfilePicture ?? string.Empty; // Retrieve the image URL, or use an empty string if not available


            try
            {
                var bookPostLike = new BookPostLike
                {
                    BookId = model.BookId,
                    UserId = model.UserId,
                    DateLiked = DateTime.Now
                };

                _context.BookPostLikes.Add(bookPostLike);
                _context.SaveChanges();
                var user = await _userManager.FindByIdAsync(model.UserId);
                //Create the notification 
                var notifiction = new Notification
                {
                    Id = 0,
                    UserId = user.Id,
                    Message = $"{user.UserName} Liked Your Post :{bookPostLike.BookId}",
                    DateSent = DateTime.Now,
                    IsRead = false,
                    imgUrl = user.ProfilePicture,
                    UserName = user.UserName,


                };

                _context.Notifications.Add(notifiction);
                _context.SaveChanges();
                //find the user that have the book



                //get the user id for the book (wech posted it)

                var LikedUser = _context.Books.Where(b => b.Id == model.BookId).Include(b => b.AppUser).FirstOrDefault().AppUser.Id;
                var notificationUser = new NotificationUser
                {
                    Notification = notifiction,
                    UserId = LikedUser,
                    NotificationId = notifiction.Id
                };


                //make a list of string with only licked user id 
                List<string> UserIds = new List<string>();

                UserIds.Add(LikedUser);

                _context.NotificationUsers.Add(notificationUser);

                _context.SaveChanges();

                foreach (var userId in UserIds)
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

                response.Success = true;

                response.Data = model;
            }
            catch (Exception ex)
            {

                response.Success = false;
                response.Message = $"Failed to create like: {ex.Message}";
            }


            return response;
        }

        public async Task<BookPostLikeModel> GetById(AddBookPostLikeModel model)
        {
            var bookPostLike = await _context.BookPostLikes
                                     .Where(bpl => bpl.UserId == model.UserId && bpl.BookId == model.BookId)
                                     .FirstOrDefaultAsync();

            if (bookPostLike == null) return null;

            return new BookPostLikeModel
            {
                Id = bookPostLike.Id,
                BookId = bookPostLike.BookId,
                UserId = bookPostLike.UserId,
                DateLiked = bookPostLike.DateLiked
            };
        }
        public async Task<List<BookPostLikeModel>> GetAll()
        {
            return await _context.BookPostLikes.Select(bookPostLike => new BookPostLikeModel
            {
                Id = bookPostLike.Id,
                BookId = bookPostLike.BookId,
                UserId = bookPostLike.UserId,
                DateLiked = bookPostLike.DateLiked
            }).ToListAsync();
        }

        public async Task<ServiceResponse<BookPostLikeModel>> Update(BookPostLikeModel model)
        {
            var response = new ServiceResponse<BookPostLikeModel>();
            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var BookPostLike = await _context.BookPostLikes.FindAsync(model.Id);
            if (BookPostLike == null)
            {
                response.Success = false;
                response.Message = "BookPostLike not found.";
                return response;
            }
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        BookPostLike.Id = model.Id;
                        BookPostLike.BookId = model.BookId;
                        BookPostLike.UserId = model.UserId;
                        BookPostLike.DateLiked = model.DateLiked;

                        _context.BookPostLikes.Update(BookPostLike);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update badge: {ex.Message}";
                    }
                }

            });
            return response;
        }

        public async Task<ServiceResponse<BookPostLikeModel>> Delete(AddBookPostLikeModel model)
        {    //who reseved the like=lkied user 
             //who did the like = model.UserId
            var LikedUser = _context.Books.Where(b => b.Id == model.BookId).Include(b => b.AppUser).FirstOrDefault().AppUser.Id;

            var response = new ServiceResponse<BookPostLikeModel>();
            var BookPostLike = _context.BookPostLikes.FirstOrDefault(bpl => bpl.UserId == model.UserId && bpl.BookId == model.BookId);
            string s = " Liked Your Post:thisisid";

            // s = s.Substring(s.IndexOf(":") + 1);


            var notification = _context.Notifications
               .Where(n => n.UserId == model.UserId && n.Message.Contains(" Liked Your Post") && n.Message.Substring(n.Message.IndexOf(":") + 1) == model.BookId.ToString())
               .SelectMany(n => n.NotificationUsers)
               .Where(nu => nu.UserId == LikedUser)
               .Select(nu => new { notificationId = nu.NotificationId })
               .FirstOrDefault();

            if (notification == null)
            {
                notification = _context.Notifications
                 .Where(n => n.UserId == LikedUser && n.Message.Contains(" Liked Your Post"))
                 .SelectMany(n => n.NotificationUsers)
                 .Where(nu => nu.UserId == model.UserId)
                 .Select(nu => new { notificationId = nu.NotificationId })
                 .FirstOrDefault();
            }


            await _notificationService.Delete(notification.notificationId);
            var UserIds = new List<string>();
            UserIds.Add(LikedUser);

            if (BookPostLike == null)
            {
                response.Success = false;
                response.Message = "BookPostLike not found.";
                return response;
            }
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {

                        _context.BookPostLikes.Remove(BookPostLike);
                        await _context.SaveChangesAsync();

                        foreach (var userId in UserIds)
                        {
                            var connections = _userConnectionsService.GetUserConnections(userId);

                            foreach (var connection in connections)
                            {
                                await _hubContext.Clients.Client(connection).SendAsync("NotificationModelDelete", new
                                {
                                    id = notification.notificationId,
                                });
                            }
                        }
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new BookPostLikeModel
                        {
                            Id = BookPostLike.Id,
                            BookId = BookPostLike.BookId,
                            UserId = BookPostLike.UserId,
                            DateLiked = BookPostLike.DateLiked
                        };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete badge: {ex.Message}";
                    }

                }
            });

            return response;



        }

        public async Task<ServiceResponse<Dictionary<int, bool>>> CheckBulkLikes(string userId, List<int> bookIds)
        {
            var results = new Dictionary<int, bool>();
            var respose = new ServiceResponse<Dictionary<int, bool>> { };
            foreach (var bookId in bookIds)
            {
                var isLiked = await _context.BookPostLikes.AnyAsync(bpl => bpl.UserId == userId && bpl.BookId == bookId);
                //check results dont already have it 
                if (!results.ContainsKey(bookId))
                {
                    results.Add(bookId, isLiked);
                }



            }
            respose.Success = true;
            respose.Data = results;
            if (results.Count == 0)
            {
                respose.Success = false;
                respose.Message = "No results found.";
            }
            return respose;
        }
        private static string ConvertToFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            var baseUrl = "https://localhost:7139";
            return $"{baseUrl}{relativePath}";
        }

    }
}
