using Lafatkotob.Entities;
using Lafatkotob.Hubs;
using Lafatkotob.Services.NotificationService;
using Lafatkotob.Services.NotificationUserService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static System.Reflection.Metadata.BlobBuilder;

namespace Lafatkotob.Services.BooksInWishlistsService
{
    public class BooksInWishlistsService : IBooksInWishlistsService
    {
        private readonly ApplicationDbContext _context;

        public BooksInWishlistsService(ApplicationDbContext context, UserManager<AppUser> userManager)
        {

            _context = context;
            

        }
        public async Task<ServiceResponse<BookInWishlistsModel>> Delete(int id)
        {
            var response = new ServiceResponse<BookInWishlistsModel>();

            var BookInWishlists = await _context.BooksInWishlists.FindAsync(id);
            if (BookInWishlists == null)
            {
                response.Success = false;
                response.Message = "BookInWishlists not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.BooksInWishlists.Remove(BookInWishlists);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new BookInWishlistsModel
                        {
                            Id = BookInWishlists.Id,
                            Title = BookInWishlists.Title,
                            Author = BookInWishlists.Author,
                            ISBN = BookInWishlists.ISBN,
                            Language = BookInWishlists.Language,
                            AddedDate = BookInWishlists.AddedDate,

                        };
                       
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete BookInWishlists: {ex.Message}";
                    }
                }
            });

            return response;
        

    }
    public async Task<List<BookInWishlistsModel>> GetAll()
        {
            return await _context.BooksInWishlists.Select(book => new BookInWishlistsModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Language = book.Language,
                AddedDate = book.AddedDate
            }).ToListAsync();
        }
        public async Task<BookInWishlistsModel> GetById(int id)
        {
            var BookInWishList = await _context.BooksInWishlists.FindAsync(id);
            if (BookInWishList == null) return null;
            return new BookInWishlistsModel
            {
                Id = BookInWishList.Id,
                Title = BookInWishList.Title,
                Author = BookInWishList.Author,
                ISBN = BookInWishList.ISBN,
                Language = BookInWishList.Language,
                AddedDate = BookInWishList.AddedDate

            };

        }
        public async Task<ServiceResponse<BookInWishlistsModel>> Post(BookInWishlistsModel model)
        {
            var response = new ServiceResponse<BookInWishlistsModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var BookInWishlists = new BooksInWishlists
                        {
                            Title = model.Title,
                            Author = model.Author,
                            ISBN = model.ISBN,
                            Language = model.Language,
                            AddedDate = DateTime.Now
                            
                        };
                       

                        _context.BooksInWishlists.Add(BookInWishlists);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception )
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = "Failed to create BookInWishlists.";
                    }
                }
            });


            return response;
        }
        public async Task<ServiceResponse<BookInWishlistsModel>> PostAll(BookInWishlistsModel model,string UserId)//added book to BookInWishlist and added it to wishedBook and if there is not wishlist for the user it create one 
        {
            var response = new ServiceResponse<BookInWishlistsModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var BookInWishlists = new BooksInWishlists
                        {
                            Title = model.Title,
                            Author = model.Author,
                            ISBN = model.ISBN,
                            Language = model.Language,
                            AddedDate = DateTime.Now

                        };
                        _context.BooksInWishlists.Add(BookInWishlists);
                        await _context.SaveChangesAsync();
                        var wishlist = _context.Wishlists.Where(w => w.UserId == UserId).FirstOrDefault();
                        if (wishlist == null) {
                            var newWishlist = new Wishlist
                            {
                                UserId = UserId
                            };
                            _context.Wishlists.Add(newWishlist);
                            await _context.SaveChangesAsync();
                            wishlist = newWishlist; 
                        }
                        else
                        {
                            _context.Wishlists.Update(wishlist);
                        }
                        var WishedBook = new WishedBook
                        {
                            WishlistId = wishlist.Id,
                            BooksInWishlistsId = BookInWishlists.Id,
                        
                         };
                        _context.WishedBooks.Add(WishedBook);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = "Failed to create BookInWishlists.";
                    }
                }
            });


            return response;
        }
        public async Task<ServiceResponse<BookInWishlistsModel>> Update(BookInWishlistsModel model)
        {
            var response = new ServiceResponse<BookInWishlistsModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var BookInWishlists = await _context.BooksInWishlists.FindAsync(model.Id);
            if (BookInWishlists == null)
            {
                response.Success = false;
                response.Message = "BookInWishlists not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        BookInWishlists.Author = model.Author;
                        BookInWishlists.Title = model.Title;
                        BookInWishlists.ISBN = model.ISBN;
                        BookInWishlists.Language = model.Language;
                        BookInWishlists.AddedDate = DateTime.Now;


                        _context.BooksInWishlists.Update(BookInWishlists);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update BookInWishlists: {ex.Message}";
                    }
                }
            });

            return response;
        }

    }
}
