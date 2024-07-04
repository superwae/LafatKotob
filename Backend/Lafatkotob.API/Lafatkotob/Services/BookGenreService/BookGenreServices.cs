using Azure;
using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static System.Reflection.Metadata.BlobBuilder;

namespace Lafatkotob.Services.BookGenreService
{
    public class BookGenreServices : IBookGenreServices
    {
        private readonly ApplicationDbContext _context;
        public BookGenreServices(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<BookGenreModel>> Post(BookGenreModel model)
        {
            var response = new ServiceResponse<BookGenreModel>();
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var BookGenre = new BookGenre
                        {

                            BookId = model.BookId,
                            GenreId = model.GenreId
                        };

                        _context.BookGenres.Add(BookGenre);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;


                    }
                    catch (Exception )
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create badge.";
                        throw;
                    }
                }
            });
            return response;
        }
        public async Task<BookGenreModel> GetById(int id)
        {
            var BookGenre = await _context.BookGenres.FindAsync(id);
            if (BookGenre == null) return null;

            return new BookGenreModel
            {
                Id = BookGenre.Id,
                BookId = BookGenre.BookId,
                GenreId = BookGenre.GenreId
            };
        }

        public async Task<List<BookGenreModel>> GetAll()
        {
            return await _context.BookGenres
                .Select(up => new BookGenreModel
                {
                    Id = up.Id,
                    BookId = up.BookId,
                    GenreId = up.GenreId
                })
                .ToListAsync();
        }
        public async Task<ServiceResponse<BookGenreModel>> Update(BookGenreModel model)
        {
            var response = new ServiceResponse<BookGenreModel>();
            if (model == null)
            {
                response.Success = false;
                response.Message = "Model is null";
                return response;
            }
            var BookGenre = await _context.BookGenres.FindAsync(model.Id);
            if (BookGenre == null)
            {
                response.Success = false;
                response.Message = "BookGenre not found";
                return response;
            }
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        BookGenre.Id = model.Id;
                        BookGenre.BookId = model.BookId;
                        BookGenre.GenreId = model.GenreId;
                        _context.BookGenres.Update(BookGenre);
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
        public async Task<ServiceResponse<BookGenreModel>> Delete(int id)
        {
            var response = new ServiceResponse<BookGenreModel>();
            var BookGenre = await _context.BookGenres.FindAsync(id);
            if (BookGenre == null)
            {
                response.Success = false;
                response.Message = "BookGenre not found";
                return response;

            }
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.BookGenres.Remove(BookGenre);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        response.Success = true;
                        response.Data = new BookGenreModel
                        {
                            Id = BookGenre.Id,
                            BookId = BookGenre.BookId,
                            GenreId = BookGenre.GenreId
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
    }
}
