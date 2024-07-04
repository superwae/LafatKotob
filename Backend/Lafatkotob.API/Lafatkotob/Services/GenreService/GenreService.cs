using Lafatkotob.Entities;
using Lafatkotob.Services.GenreService;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.GenreService
{
    public class GenreService : IGenreService
    {
        private readonly ApplicationDbContext _context;

        public GenreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<GenreModel>> Post(GenreModel model)
        {
            var response = new ServiceResponse<GenreModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        var Genre = new Genre
                        {
                            Name = model.Name
                        };
                        
                        _context.Genres.Add(Genre);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create Genre.";
                    }
                }
            });

            return response;
        }

        public async Task<GenreModel> GetById(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return null;

            return new GenreModel
            {
                Id = genre.Id,
                Name = genre.Name
            };
        }

        public async Task<List<GenreModel>> GetAll()
        {
            return await _context.Genres
                .Select(g => new GenreModel
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<GenreModel>> Update(GenreModel model)
        {
            var response = new ServiceResponse<GenreModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var Genre = await _context.Genres.FindAsync(model.Id);
            if (Genre == null)
            {
                response.Success = false;
                response.Message = "Genre not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        Genre.Name = model.Name;

                        _context.Genres.Update(Genre);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update Genre: {ex.Message}";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<GenreModel>> Delete(int id)
        {
            var response = new ServiceResponse<GenreModel>();

            var Genre = await _context.Genres.FindAsync(id);
            if (Genre == null)
            {
                response.Success = false;
                response.Message = "Genre not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Genres.Remove(Genre);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new GenreModel
                        {
                            Id = Genre.Id,
                            Name = Genre.Name
                        };
                        
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete Genre: {ex.Message}";
                    }
                }
            });

            return response;
        }
    }
}
