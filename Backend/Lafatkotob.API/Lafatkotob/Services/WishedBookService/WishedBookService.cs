using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lafatkotob.Services.WishedBookService
{
    public class WishedBookService : IWishedBookService
    {
        private readonly ApplicationDbContext _context;

        public WishedBookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<WishedBookModel>> Delete(int id)
        {
            var response = new ServiceResponse<WishedBookModel>();

            var WishedBook = await _context.WishedBooks.FindAsync(id);
            if (WishedBook == null)
            {
                response.Success = false;
                response.Message = "WishedBook not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.WishedBooks.Remove(WishedBook);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new WishedBookModel
                        {
                            Id = WishedBook.Id,
                            BooksInWishlistsId = WishedBook.BooksInWishlistsId,
                            WishlistId = WishedBook.WishlistId
                        };
                      
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete WishedBook: {ex.Message}";
                    }
                }
            });

            return response;
        }

        public async Task<WishedBookModel> GetById(int id)
        {
            var wishedBook = await _context.WishedBooks.FindAsync(id);
            if (wishedBook == null) return null;

            return new WishedBookModel
            {
                Id = wishedBook.Id,
                BooksInWishlistsId = wishedBook.BooksInWishlistsId,
                WishlistId = wishedBook.WishlistId
            };
        }

        public async Task<List<WishedBookModel>> GetAll()
        {
            return await _context.WishedBooks
                .Select(wb => new WishedBookModel
                {
                    Id = wb.Id,
                    BooksInWishlistsId = wb.BooksInWishlistsId,
                    WishlistId = wb.WishlistId
                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<WishedBookModel>> Post(WishedBookModel model)
        {
            var response = new ServiceResponse<WishedBookModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        var WishedBook = new WishedBook
                        {
                           BooksInWishlistsId = model.BooksInWishlistsId,
                           WishlistId = model.WishlistId

                        };
                        _context.WishedBooks.Add(WishedBook);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create WishedBook.";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<WishedBookModel>> Update(WishedBookModel model)
        {
            var response = new ServiceResponse<WishedBookModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "WishedBook cannot be null.";
                return response;
            }

            var WishedBook = await _context.WishedBooks.FindAsync(model.Id);
            if (WishedBook == null)
            {
                response.Success = false;
                response.Message = "WishedBook not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        WishedBook.BooksInWishlistsId = model.BooksInWishlistsId;
                        WishedBook.WishlistId = model.WishlistId;

                        _context.WishedBooks.Update(WishedBook);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update WishedBook: {ex.Message}";
                    }
                }
            });

            return response;
        }
    }
}
