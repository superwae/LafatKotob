using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lafatkotob.Services.WishListService
{
    public class WishListService : IWishListService
    {
        private readonly ApplicationDbContext _context;

        public WishListService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<WishlistModel>> Post(WishlistModel model)
        {
            var response = new ServiceResponse<WishlistModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        var Wishlist = new Wishlist
                        {
                            UserId = model.UserId,
                            DateAdded = model.DateAdded
                        };
                       
                        _context.Wishlists.Add(Wishlist);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create Wishlist.";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<WishlistModel>> GetById(int id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            var response = new ServiceResponse<WishlistModel>();

            if (wishlist == null)
            {
                response.Success = false;
                response.Message = "Wishlist not found.";
                return response;
            }

            response.Data = new WishlistModel
            {
                Id = wishlist.Id,
                UserId = wishlist.UserId,
                DateAdded = wishlist.DateAdded,
            };

            return response;
        }

        public async Task<ServiceResponse<List<WishlistModel>>> GetAll()
        {
            var response = new ServiceResponse<List<WishlistModel>>
            {
                Data = await _context.Wishlists
                            .Select(wl => new WishlistModel
                            {
                                Id = wl.Id,
                                UserId = wl.UserId,
                                DateAdded = wl.DateAdded,
                            })
                            .ToListAsync()
            };

            return response;
        }

        public async Task<ServiceResponse<WishlistModel>> Update(WishlistModel model)
        {
            var response = new ServiceResponse<WishlistModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Wishlist cannot be null.";
                return response;
            }

            var Wishlist = await _context.Wishlists.FindAsync(model.Id);
            if (Wishlist == null)
            {
                response.Success = false;
                response.Message = "Wishlist not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        Wishlist.DateAdded = model.DateAdded;


                        _context.Wishlists.Update(Wishlist);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update Wishlist: {ex.Message}";
                    }
                }
            });

            return response;
        }
        public async Task<ServiceResponse<WishlistModel>> Delete(int id)
        {
            var response = new ServiceResponse<WishlistModel>();
            var Wishlist = await _context.Wishlists.FindAsync(id);

            if (Wishlist == null)
            {
                response.Success = false;
                response.Message = "Wishlist not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Wishlists.Remove(Wishlist);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new WishlistModel
                        {
                            Id = Wishlist.Id,
                            UserId = Wishlist.UserId,
                            DateAdded = Wishlist.DateAdded
                        };
                       
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete wishlist: {ex.Message}";
                    }
                }
            });

            return response;
        }


        public async Task<ServiceResponse<List<BookInWishlistsModel>>> GetByUserId(string userId)
        {
            var wishlist = await _context.Wishlists.FirstOrDefaultAsync(wl => wl.UserId == userId);

            var books = await _context.WishedBooks
                             .Where(wl => wl.WishlistId == wishlist.Id)
                              .Select(wb => new BookInWishlistsModel
                              {
                                  Id = wb.BooksInWishlists.Id,
                                  Title = wb.BooksInWishlists.Title,
                                  Author = wb.BooksInWishlists.Author,
                                  ISBN = wb.BooksInWishlists.ISBN,
                                  Language = wb.BooksInWishlists.Language,
                                  AddedDate = wb.BooksInWishlists.AddedDate
                              }).ToListAsync();

            var response = new ServiceResponse<List<BookInWishlistsModel>>
            {
                Data = books
            };
            if (response.Data.Count == 0)
            {
                response.Message = "No books found in wishlist";
                response.Success = false;
                return response;

            }
            response.Success = true;


            return response;
        }



    }
}
