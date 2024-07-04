using Azure;
using Lafatkotob.Entities;
using Lafatkotob.Hubs;
using Lafatkotob.Services.NotificationService;
using Lafatkotob.Services.NotificationUserService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;
using Lafatkotob.Hubs;
using Lafatkotob.Services.UserBadgeService;
namespace Lafatkotob.Services.BookService

{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly INotificationUserService _notificationUserService;
        private readonly IUserConnectionsService _userConnectionsService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUserBadgeService _userBadge;
        public BookService(ApplicationDbContext context, IUserBadgeService userBadge, IHubContext<ChatHub> hubContext, IUserConnectionsService userConnectionsService, UserManager<AppUser> userManager, INotificationService notificationService, INotificationUserService notificationUserService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _notificationUserService = notificationUserService;
            _hubContext = hubContext;
            _userConnectionsService = userConnectionsService;
              _userBadge = userBadge;
        }


        public async Task<ServiceResponse<BookModel>> Post(RegisterBook model, IFormFile imageFile)
        {
            var response = new ServiceResponse<BookModel>();
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var imagePath = await SaveImageAsync(imageFile);

                        var book = new Book
                        {
                            Title = model.Title,
                            Author = model.Author,
                            Description = model.Description,
                            UserId = model.UserId,
                            HistoryId = model.HistoryId,
                            PublicationDate = model.PublicationDate,
                            ISBN = model.ISBN,
                            PageCount = model.PageCount,
                            Condition = model.Condition,
                            Type = model.Type,
                            Status = model.Status,
                            PartnerUserId = model.PartnerUserId,
                            CoverImage = imagePath,
                            Language = model.Language,
                            AddedDate = DateTime.Now

                        };


                        _context.Books.Add(book);
                        await _context.SaveChangesAsync();
                        var BookModel = new BookModel
                        {
                            Id = book.Id,
                            Title = book.Title,
                            Author = book.Author,
                            Description = book.Description,
                            CoverImage = book.CoverImage,
                            UserId = book.UserId,
                            HistoryId = book.HistoryId,
                            PublicationDate = book.PublicationDate,
                            ISBN = book.ISBN,
                            PageCount = book.PageCount,
                            Condition = book.Condition,
                            Status = book.Status,
                            Type = book.Type,
                            PartnerUserId = book.PartnerUserId,
                            Language = book.Language,
                            AddedDate = DateTime.Now
                        };
                        await transaction.CommitAsync();

                        BookModel.CoverImage = imagePath; // Update model with image path for response
                        response.Success = true;
                        response.Data = BookModel;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to create book: {ex.Message}";
                    }
                }
            });

            return response;
        }
        public async Task<GetGenreForUserModel> GetGenresForUser(string userId)
        {
            var response = new ServiceResponse<GetGenreForUserModel>();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response.Data;
                }
                List<string> genreNamesForMyBook = await _context.BookGenres
                    .Where(bg => bg.Book.UserId == userId)
                    .Select(bg => bg.Genre.Name)
                    .ToListAsync();
                //i want to get the genres of the books that the user liked
               List<string> genreNamesForLikedBooks = await _context.Books
                    .Where(b => b.BookPostLikes.Any(bpl => bpl.UserId == userId))
                    .SelectMany(b => b.BookGenres)
                    .Select(bg => bg.Genre.Name)
                    .ToListAsync();

                var model = new GetGenreForUserModel
                {
                    user_id = userId,
                    books = new List<BookGenres>
                    {

                      new BookGenres {genres = genreNamesForMyBook } ,
                      new BookGenres {genres = genreNamesForLikedBooks

                    }
            }
                  
                };
                response.Success = true;
                response.Data = model;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to get genres for user: {ex.Message}";
            }
            return response.Data;

        
        }

        public async Task<ServiceResponse<BookModel>> Report(int id, string userId)
        {
            var response = new ServiceResponse<BookModel>();
            try
            {


                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    response.Success = false;
                    response.Message = "book not found.";
                    return response;
                }

                response.Success = true;
                response.Data = new BookModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Description = book.Description,
                    CoverImage = book.CoverImage,
                    UserId = book.UserId,
                    HistoryId = book.HistoryId,
                    PublicationDate = book.PublicationDate,
                    ISBN = book.ISBN,
                    PageCount = book.PageCount,
                    Condition = book.Condition,
                    Status = book.Status,
                    Type = book.Type,
                    PartnerUserId = book.PartnerUserId,
                    Language = book.Language,
                    AddedDate = book.AddedDate,

                };
                var admenIds = await _userManager.GetUsersInRoleAsync("Admin");
                var reporedUser = await _userManager.FindByIdAsync(userId);
                var notification = new Notification
                {

                    Message = $"The book {book.Title} has been reported :" + id.ToString(),
                    UserId = userId,
                    DateSent = DateTime.Now,
                    IsRead = false,
                    imgUrl = book.CoverImage,
                    UserName = reporedUser.UserName

                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                var notificationId = _context.Notifications.Where(nu => nu.Message.Contains($"The book {book.Title} has been reported :" + id.ToString())).FirstOrDefault();
                List<string>users= new List<string>();

                for (int i = 0; i < admenIds.Count; i++)
                {
                    var notificationUser = new NotificationUser
                    {
                        UserId = admenIds[i].Id,
                        NotificationId = notificationId.Id,

                    };
                    users.Add(admenIds[i].Id);
                    _context.NotificationUsers.Add(notificationUser);
                    await _context.SaveChangesAsync();
                }

                foreach (var userId2 in users)
                {
                    var connections = _userConnectionsService.GetUserConnections(userId2);

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
                response.Success = false;
                response.Message = $"Failed to report book: {ex.Message}";
            }
            return response;

        }
        public async Task<ServiceResponse<BookWithUserModel>> RegisterBookWithGenres(RegisterBookWithGenres model, IFormFile imageFile)
        {
            var response = new ServiceResponse<BookWithUserModel>();
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var history = await _context.History.FirstOrDefaultAsync(h => h.UserId == model.UserId);
                        int historyId = 0;
                        if (history == null)
                        {
                            var newHistory = new History
                            {
                                UserId = model.UserId,
                            };
                            _context.History.Add(newHistory);
                            await _context.SaveChangesAsync();
                            historyId = newHistory.Id;
                        }
                        else
                        {
                            historyId = history.Id;
                        }

                        var imagePath = await SaveImageAsync(imageFile);

                        var book = new Book
                        {
                            Title = model.Title,
                            Author = model.Author,
                            Description = model.Description,
                            UserId = model.UserId,
                            HistoryId = historyId,
                            PublicationDate = model.PublicationDate,
                            ISBN = model.ISBN,
                            PageCount = model.PageCount,
                            Condition = model.Condition,
                            Type = model.Type,
                            Status = model.Status,
                            PartnerUserId = model.PartnerUserId,
                            CoverImage = imagePath,
                            Language = model.Language,
                            AddedDate = DateTime.Now
                        };

                        _context.Books.Add(book);
                        await _context.SaveChangesAsync();
                        model.BookId = book.Id;
                        List<int> genreIds = JsonSerializer.Deserialize<List<int>>(model.GenreIds);
                        foreach (var genreId in genreIds)
                        {
                            var bookGenre = new BookGenre
                            {
                                BookId = book.Id,
                                GenreId = genreId
                            };
                            _context.BookGenres.Add(bookGenre);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        model.CoverImage = ConvertToFullUrl(imagePath);
                        model.Id = book.Id;
                        response.Success = true;
                        var user = await _userManager.FindByIdAsync(model.UserId);
                        var bookModel = new BookWithUserModel
                        {
                            Id = book.Id,
                            Title = book.Title,
                            Author = book.Author,
                            Description = book.Description,
                            CoverImage = ConvertToFullUrl(book.CoverImage),
                            UserId = book.UserId,
                            HistoryId = book.HistoryId,
                            PublicationDate = book.PublicationDate,
                            ISBN = book.ISBN,
                            PageCount = book.PageCount,
                            Condition = book.Condition,
                            Status = book.Status,
                            Type = book.Type,
                            PartnerUserId = book.PartnerUserId,
                            Language = book.Language,
                            AddedDate = book.AddedDate,
                            UserName = user.UserName,
                            UserProfilePicture = ConvertToFullUrl(user.ProfilePicture),

                        };
                        response.Data = bookModel;
                        //the book in wishlist that may interest the user// the users that have this book in there wishlist


                        var IfbookInWishListUsersId = await _context.WishedBooks
                       .Where(wb => wb.BooksInWishlists.Title == model.Title && wb.BooksInWishlists.Author == model.Author)
                       .Select(wb => wb.Wishlist.UserId)
                       .Distinct()
                       .ToListAsync();

                        //the notification i want to send
                        var notificationResponse = await _notificationService.Post(new NotificationModel
                        {

                            UserId = model.UserId,
                            Message = user.UserName + " Post a book that may interest you  :" + model.BookId,
                            ImgUrl = user.ProfilePicture,
                            DateSent = DateTime.Now,
                            IsRead = false,
                            UserName = user.UserName,
                        });

                        List<string> users = new List<string>();
                        //send the notification to the users
                        for (int i = 0; i < IfbookInWishListUsersId.Count; i++)
                        {
                            await _notificationUserService.Post(new NotificationUserModel
                            {
                                NotificationId = notificationResponse.Data.Id,
                                UserId = IfbookInWishListUsersId[i],

                            });
                            users.Add(IfbookInWishListUsersId[i]);

                        }
                        Notification notification2 = new Notification
                        {
                            Message = notificationResponse.Data.Message,
                            UserId = notificationResponse.Data.UserId,
                            DateSent = notificationResponse.Data.DateSent,
                            IsRead = false,
                            imgUrl = notificationResponse.Data.ImgUrl,
                            UserName = notificationResponse.Data.UserName
                        };

                        foreach (var userId2 in users)
                        {
                            var connections = _userConnectionsService.GetUserConnections(userId2);

                            foreach (var connection in connections)
                            {
                                await _hubContext.Clients.Client(connection).SendAsync("NotificationModel", new
                                {
                                    id = notification2.Id,
                                    message = notification2.Message,
                                    dateSent = notification2.DateSent,
                                    isRead = notification2.IsRead,
                                    userId = notification2.UserId,
                                    userName = notification2.UserName,
                                    imgUrl = ConvertToFullUrl(notification2.imgUrl)
                                });
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to create book: {ex.Message}";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<Book>> UpdateBookWithGenres(RegisterBookWithGenres model, IFormFile imageFile)
        {
            var response = new ServiceResponse<Book>();
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var book = await _context.Books.Include(b => b.BookGenres).FirstOrDefaultAsync(b => b.Id == model.BookId);
                        if (book == null)
                        {
                            response.Success = false;
                            response.Message = "Book not found";
                        }

                        book.Title = model.Title;
                        book.Author = model.Author;
                        book.Description = model.Description;
                        book.PublicationDate = model.PublicationDate;
                        book.ISBN = model.ISBN;
                        book.PageCount = model.PageCount;
                        book.Condition = model.Condition;
                        book.Type = model.Type;
                        book.Status = model.Status;
                        book.Language = model.Language;

                        // Update the image if a new one is provided
                        if (imageFile != null)
                        {
                            book.CoverImage = await SaveImageAsync(imageFile);
                        }

                        // Update genres
                        List<int> newGenreIds = JsonSerializer.Deserialize<List<int>>(model.GenreIds);
                        var currentGenres = book.BookGenres.Select(bg => bg.GenreId).ToList();

                        // Remove old genres
                        foreach (var genre in book.BookGenres.Where(bg => !newGenreIds.Contains(bg.GenreId)).ToList())
                        {
                            _context.BookGenres.Remove(genre);
                        }

                        // Add new genres
                        foreach (var genreId in newGenreIds.Where(gid => !currentGenres.Contains(gid)))
                        {
                            var newBookGenre = new BookGenre
                            {
                                BookId = book.Id,
                                GenreId = genreId
                            };
                            _context.BookGenres.Add(newBookGenre);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        response.Success = true;
                        response.Data = book;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update book: {ex.Message}";
                    }
                }
            });

            return response;
        }
        public async Task<ServiceResponse<Book>> SellBook(int bookId)
        {
            var response = new ServiceResponse<Book>();
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var book = await _context.Books.FirstAsync(b => b.Id == bookId);
                        if (book == null)
                        {
                            response.Success = false;
                            response.Message = "Book not found";
                        }
                        book.PartnerUserId = book.UserId;
                        book.Status = "unavailable";
                        await _context.SaveChangesAsync();

                        var tradingCount = await _context.Books
                            .Where(b => b.UserId == b.PartnerUserId&&b.UserId==book.UserId)
                            .CountAsync();

                        if (tradingCount >= 10)
                        {
                            var badge = await _context.Badges.Where(b => b.BadgeName == "TradingGold").FirstOrDefaultAsync();
                            var userbadge = await _context.UserBadges.Where(us => us.BadgeId == badge.Id && us.UserId == book.UserId)
                                                        .Select(b => new UserBadgeModel
                                                        {
                                                            UserId = b.UserId,
                                                            BadgeId = b.BadgeId,
                                                            DateEarned = b.DateEarned,

                                                        })
                                                       .FirstOrDefaultAsync(); 
                            if (userbadge == null)
                            {
                                var userBadge = new UserBadgeModel
                                {
                                    BadgeId = badge.Id,
                                    UserId = book.UserId,
                                    DateEarned = DateTime.Now

                                };
                                _userBadge.Post(userBadge);
                            }

                        }
                        else if (tradingCount >= 5)
                        {
                            
                            var badge = await _context.Badges.Where(b => b.BadgeName == "TradingSilver").FirstOrDefaultAsync();

                            var userbadge = await _context.UserBadges.Where(us => us.BadgeId == badge.Id && us.UserId == book.UserId)
                             .Select(b => new UserBadgeModel
                             {
                                 UserId = b.UserId,
                                 BadgeId = b.BadgeId,
                                 DateEarned = b.DateEarned,

                             })
                            .FirstOrDefaultAsync();
                            if (userbadge == null)
                            {

                                var userBadge = new UserBadgeModel
                                {
                                    BadgeId = badge.Id,
                                    UserId = book.UserId,
                                    DateEarned = DateTime.Now

                                };
                                _userBadge.Post(userBadge);
                            }
                        }
                        else if (tradingCount >= 1)
                        {
                            var badge = await _context.Badges.Where(b => b.BadgeName == "TradingBronze").FirstOrDefaultAsync();

                       
                            var userbadgee = await _context.UserBadges.Where(us => us.BadgeId == badge.Id && us.UserId == book.UserId)
                             .Select(b => new UserBadgeModel
                             {
                                 UserId = b.UserId,
                                 BadgeId = b.BadgeId,
                                 DateEarned = b.DateEarned,

                             })
                            .FirstOrDefaultAsync();
                            if (userbadgee == null)
                            {
                                var userBadge = new UserBadgeModel
                                {
                                    BadgeId = badge.Id,
                                    UserId = book.UserId,
                                    DateEarned = DateTime.Now

                                };
                                _userBadge.Post(userBadge);
                            }
                        }

                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        response.Success = true;
                        response.Data = book;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update book: {ex.Message}";
                    }
                }
            });

            return response;
        }


        public async Task<BookWithUserModel> GetById(int id)
        {
            var book = await _context.Books
                 .Include(b => b.AppUser)
                 .Where(b => b.Id == id)
                 .Select(up => new BookWithUserModel
                 {
                     Id = up.Id,
                     Title = up.Title,
                     Author = up.Author,
                     Description = up.Description,
                     CoverImage = ConvertToFullUrl(up.CoverImage),
                     UserId = up.UserId,
                     HistoryId = up.HistoryId,
                     PublicationDate = up.PublicationDate,
                     ISBN = up.ISBN,
                     PageCount = up.PageCount,
                     Condition = up.Condition,
                     Status = up.Status,
                     Type = up.Type,
                     PartnerUserId = up.PartnerUserId,
                     Language = up.Language,
                     AddedDate = up.AddedDate,
                     UserName = up.AppUser.UserName,
                     UserProfilePicture = ConvertToFullUrl(up.AppUser.ProfilePicture),
                 })
                 .FirstOrDefaultAsync();
            return book;
        }


        public async Task<List<BookModel>> GetBooksByUserName(string username, int pageNumber, int pageSize)
        {
            var userid = _context.Users.Where(u => u.UserName == username).Select(u => u.Id).FirstOrDefault();
            var books = await _context.Books
                .Where(b => b.UserId == userid)
                .OrderByDescending(b => b.UserId != b.PartnerUserId)
                .ThenByDescending(b => b.AddedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Description = b.Description,
                    CoverImage = ConvertToFullUrl(b.CoverImage),
                    UserId = b.UserId,
                    HistoryId = b.HistoryId,
                    PublicationDate = b.PublicationDate,
                    ISBN = b.ISBN,
                    PageCount = b.PageCount,
                    Condition = b.Condition,
                    Status = b.Status,
                    Type = b.Type,
                    PartnerUserId = b.PartnerUserId,
                    Language = b.Language,
                    AddedDate = DateTime.Now
                })
                .ToListAsync();

            return books;
        }


        public async Task<List<BookModel>> GetAll(int pageNumber, int pageSize)
        {

            return await _context.Books
                .OrderByDescending(b => b.AddedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
          .Select(up => new BookModel
          {
              Id = up.Id,
              Title = up.Title,
              Author = up.Author,
              Description = up.Description,
              CoverImage = ConvertToFullUrl(up.CoverImage),
              UserId = up.UserId,
              HistoryId = up.HistoryId,
              PublicationDate = up.PublicationDate,
              ISBN = up.ISBN,
              PageCount = up.PageCount,
              Condition = up.Condition,
              Status = up.Status,
              Type = up.Type,
              PartnerUserId = up.PartnerUserId,
              Language = up.Language,
              AddedDate = DateTime.Now
          })
              .ToListAsync();
        }


        public async Task<List<BookWithUserModel>> GetAllWithUserInfo(int pageNumber, int pageSize)
        {
            return await _context.Books
                .Where(b => b.UserId != b.PartnerUserId)
                .Include(b => b.AppUser)
                .OrderByDescending(b => b.AddedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(up => new BookWithUserModel
                {
                    Id = up.Id,
                    Title = up.Title,
                    Author = up.Author,
                    Description = up.Description,
                    CoverImage = ConvertToFullUrl(up.CoverImage),
                    UserId = up.UserId,
                    HistoryId = up.HistoryId,
                    PublicationDate = up.PublicationDate,
                    ISBN = up.ISBN,
                    PageCount = up.PageCount,
                    Condition = up.Condition,
                    Status = up.Status,
                    Type = up.Type,
                    PartnerUserId = up.PartnerUserId,
                    Language = up.Language,
                    AddedDate = up.AddedDate,
                    UserName = up.AppUser.UserName,
                    UserProfilePicture = ConvertToFullUrl(up.AppUser.ProfilePicture),
                })
                .ToListAsync();
        }


        public async Task<ServiceResponse<List<Book>>> SearchBooks(string query, int pageNumber, int pageSize)
        {
            var response = new ServiceResponse<List<Book>>();
            query = query.ToLower().Trim();

            var booksQuery = _context.Books
                .Where(b => b.Title.ToLower().Contains(query) || b.Author.ToLower().Contains(query));

            response.TotalItems = await booksQuery.CountAsync(); // Total items for pagination info
            var books = await booksQuery
                .OrderBy(b => b.Title) // Optional: Order by title or another attribute
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            string baseUrl = "https://localhost:7139";
            books.ForEach(b =>
            {
                b.CoverImage = ConvertToFullUrl(b.CoverImage);      
            });

            response.Success = true;
            response.Data = books;
            if (books.Count == 0)
            {
                response.Message = "No books found matching the search criteria.";
                response.Success = false;
            }
            return response;
        }




        private string FormatCoverImageUrl(string baseUrl, string coverImagePath)
        {
            if (!string.IsNullOrWhiteSpace(coverImagePath) && !coverImagePath.StartsWith("http"))
            {
                return $"{baseUrl}{(coverImagePath.StartsWith('/') ? "" : "/")}{coverImagePath}";
            }
            return coverImagePath;
        }



        public async Task<ServiceResponse<UpdateBookModel>> Update(int id, UpdateBookModel model, IFormFile imageFile = null)
        {
            var response = new ServiceResponse<UpdateBookModel>();
            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                response.Success = false;
                response.Message = "Book not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        book.Title = model.Title ?? book.Title;
                        book.Author = model.Author ?? book.Author;
                        book.Description = model.Description ?? book.Description;
                        if (imageFile != null)
                        {
                            var imagePath = await SaveImageAsync(imageFile);
                            book.CoverImage = imagePath;
                        }
                        book.ISBN = model.ISBN ?? book.ISBN;
                        book.PageCount = model.PageCount ?? book.PageCount;
                        book.Condition = model.Condition ?? book.Condition;
                        book.Status = model.Status ?? book.Status;
                        book.Type = model.Type ?? book.Type;
                        book.Language = model.Language;
                        book.AddedDate = DateTime.Now;


                        _context.Books.Update(book);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update book: {ex.Message}";
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

        public async Task<ServiceResponse<BookModel>> DeleteByAdmin(int id)
        {
            var response = new ServiceResponse<BookModel>();
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                response.Success = false;
                response.Message = "book not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var userPosetTheBook = await _userManager.FindByIdAsync(book.UserId);
                        var notificaton = new Notification
                        {
                            Message = $"Your book {book.Title} has been deleted",
                            UserId = book.UserId,
                            DateSent = DateTime.Now,
                            IsRead = false,
                            imgUrl = book.CoverImage,
                            UserName = userPosetTheBook.UserName
                        };

                        _context.Notifications.Add(notificaton);
                        await _context.SaveChangesAsync();
                        var notificationUser = new NotificationUser
                        {
                            UserId = book.UserId,
                            NotificationId = notificaton.Id
                        };
                        _context.NotificationUsers.Add(notificationUser);
                        await _context.SaveChangesAsync();

                        var users = new List<string> { book.UserId };
                        foreach (var userId in users)
                        {
                            var connections = _userConnectionsService.GetUserConnections(userId);

                            foreach (var connection in connections)
                            {
                                await _hubContext.Clients.Client(connection).SendAsync("NotificationModel", new
                                {
                                    id = notificaton.Id,
                                    message = notificaton.Message,
                                    dateSent = notificaton.DateSent,
                                    isRead = notificaton.IsRead,
                                    userId = notificaton.UserId,
                                    userName = notificaton.UserName,
                                    imgUrl = ConvertToFullUrl(notificaton.imgUrl)
                                });
                            }
                        }
                       
                        _context.Books.Remove(book);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        response.Success = true;
                        response.Data = new BookModel
                        {
                            Id = book.Id,
                            Title = book.Title,
                            Author = book.Author,
                            Description = book.Description,
                            CoverImage = book.CoverImage,
                            UserId = book.UserId,
                            HistoryId = book.HistoryId,
                            PublicationDate = book.PublicationDate,
                            ISBN = book.ISBN,
                            PageCount = book.PageCount,
                            Condition = book.Condition,
                            Status = book.Status,
                            Type = book.Type,
                            PartnerUserId = book.PartnerUserId,
                            Language = book.Language,
                            AddedDate = book.AddedDate
                        };

                        //users id that get the notification

                        var IfbookInWishListUsersId = await _context.WishedBooks
                        .Where(wb => wb.BooksInWishlists.Title == book.Title && wb.BooksInWishlists.Author == book.Author)
                        .Select(wb => wb.Wishlist.UserId)
                        .Distinct()
                        .ToListAsync();

                        // id of the notification related to the book 
                        var notificationId = _context.Notifications
                         .Where(n => n.Message.Contains(" Post a book that may interest you") && n.Message.Substring(n.Message.IndexOf(":") + 1) == book.Id.ToString())
                         .FirstOrDefault();

                        var NotificationUsersToDelete = await _context.NotificationUsers.Where(nu => nu.NotificationId == notificationId.Id).ToListAsync();

                        //delete the notification to the users
                        for (int i = 0; i < NotificationUsersToDelete.Count; i++)
                            await _notificationUserService.Delete(NotificationUsersToDelete[i].Id);



                    }

                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete book: {ex.Message}";
                    }

                }
            });
            return response;
        }
        public async Task<ServiceResponse<BookModel>> Delete(int id)
        {
            var response = new ServiceResponse<BookModel>();
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                response.Success = false;
                response.Message = "book not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Books.Remove(book);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        response.Success = true;
                        response.Data = new BookModel
                        {
                            Id = book.Id,
                            Title = book.Title,
                            Author = book.Author,
                            Description = book.Description,
                            CoverImage = book.CoverImage,
                            UserId = book.UserId,
                            HistoryId = book.HistoryId,
                            PublicationDate = book.PublicationDate,
                            ISBN = book.ISBN,
                            PageCount = book.PageCount,
                            Condition = book.Condition,
                            Status = book.Status,
                            Type = book.Type,
                            PartnerUserId = book.PartnerUserId,
                            Language = book.Language,
                            AddedDate = book.AddedDate
                        };

                        //users id that get the notification

                        var IfbookInWishListUsersId = await _context.WishedBooks
                        .Where(wb => wb.BooksInWishlists.Title == book.Title && wb.BooksInWishlists.Author == book.Author)
                        .Select(wb => wb.Wishlist.UserId)
                        .Distinct()
                        .ToListAsync();

                        // id of the notification related to the book 
                        var notificationId = _context.Notifications
                         .Where(n => n.Message.Contains(" Post a book that may interest you") && n.Message.Substring(n.Message.IndexOf(":") + 1) == book.Id.ToString())
                         .FirstOrDefault();

                        var NotificationUsersToDelete = await _context.NotificationUsers.Where(nu => nu.NotificationId == notificationId.Id).ToListAsync();

                        //delete the notification to the users
                        for (int i = 0; i < NotificationUsersToDelete.Count; i++)
                            await _notificationUserService.Delete(NotificationUsersToDelete[i].Id);



                    }

                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete book: {ex.Message}";
                    }

                }
            });
            return response;
        }
        private static string ConvertToFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            var amazonBaseUrl = "http://images.amazon.com/images";
            if (relativePath.StartsWith(amazonBaseUrl))
            {
                return relativePath;
            }

            var baseUrl = "https://localhost:7139";
            return $"{baseUrl}{relativePath}";
        }


        public async Task<ServiceResponse<List<BookModel>>> GetBooksFilteredByGenres(List<int> genreIds, int pageNumber, int pageSize)
        {
            var response = new ServiceResponse<List<BookModel>>();

            var query = _context.Books
                .Where(b => b.BookGenres.Any(bg => genreIds.Contains(bg.GenreId)))
                .Select(b => new BookModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Description = b.Description,
                    CoverImage = ConvertToFullUrl(b.CoverImage),
                    UserId = b.UserId,
                    HistoryId = b.HistoryId,
                    PublicationDate = b.PublicationDate,
                    ISBN = b.ISBN,
                    PageCount = b.PageCount,
                    Condition = b.Condition,
                    Status = b.Status,
                    Type = b.Type,
                    PartnerUserId = b.PartnerUserId,
                    Language = b.Language,
                    AddedDate = b.AddedDate
                });
            response.TotalItems = await query.CountAsync();
            response.Data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            response.Success = true;
            if (!response.Data.Any())
            {
                response.Message = "No books found for the specified genres.";
                response.Success = false;
            }

            return response;
        }


        public async Task<ServiceResponse<List<GenreModel>>> GetGenresByBookId(int bookId)
        {
            var response = new ServiceResponse<List<GenreModel>>();
            var geners = await _context.BookGenres
                   .Where(bg => bg.BookId == bookId)
                   .Select(bg => bg.Genre)
                   .Select(g => new GenreModel
                   {
                       Id = g.Id,
                       Name = g.Name,
                   })
                   .ToListAsync();
            response.Data = geners;
            response.Success = true;
            if (geners.Count == 0)
            {
                response.Message = "No genres found for the specified book.";
                response.Success = false;
                return response;
            }
            return response;

        }
        public async Task<ServiceResponse<List<BookModel>>> GetBooksByIsbn(List<string> isbnList)
        {
            var response = new ServiceResponse<List<BookModel>>();
            try
            {
                var books = await _context.Books
                    .Where(b => isbnList.Contains(b.ISBN))
                    .Select(b => new BookModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Author = b.Author,
                        Description = b.Description,
                        CoverImage = ConvertToFullUrl(b.CoverImage),
                        UserId = b.UserId,
                        HistoryId = b.HistoryId,
                        PublicationDate = b.PublicationDate,
                        ISBN = b.ISBN,
                        PageCount = b.PageCount,
                        Condition = b.Condition,
                        Status = b.Status,
                        Type = b.Type,
                        PartnerUserId = b.PartnerUserId,
                        Language = b.Language,
                        AddedDate = DateTime.Now
                    })
                    .ToListAsync();

                response.Data = books;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }
            return response;
        }

    }
}
