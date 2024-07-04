using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.UserPreferenceService
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly ApplicationDbContext _context;

        public UserPreferenceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<UserPreferenceModel>> Delete(int id)
        {
            var response = new ServiceResponse<UserPreferenceModel>();

            var UserPreference = await _context.UserPreferences.FindAsync(id);
            if (UserPreference == null)
            {
                response.Success = false;
                response.Message = "UserPreference not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.UserPreferences.Remove(UserPreference);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new UserPreferenceModel
                        {
                            Id = UserPreference.Id,
                            UserId = UserPreference.UserId,
                            GenreId = UserPreference.GenreId,
                            PreferredAuthor = UserPreference.PreferredAuthor
                        };
                        
                       
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete UserPreference: {ex.Message}";
                    }
                }
            });

            return response;
        }

        public async Task<UserPreferenceModel> GetById(int id)
        {
            var userPreference = await _context.UserPreferences.FindAsync(id);
            if (userPreference == null) return null;

            return new UserPreferenceModel
            {
                Id = userPreference.Id,
                UserId = userPreference.UserId,
                GenreId = userPreference.GenreId,
                PreferredAuthor = userPreference.PreferredAuthor
            };
        }

        public async Task<List<UserPreferenceModel>> GetAll()
        {
            return await _context.UserPreferences
                .Select(up => new UserPreferenceModel
                {
                    Id = up.Id,
                    UserId = up.UserId,
                    GenreId = up.GenreId,
                    PreferredAuthor = up.PreferredAuthor
                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<List<UserPreferenceModel>>> PostBatch(List<UserPreferenceModel> models)
        {
            var response = new ServiceResponse<List<UserPreferenceModel>>();
            var userPreferences = new List<UserPreference>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var model in models)
                        {
                            var userPreference = new UserPreference
                            {
                                UserId = model.UserId,
                                GenreId = model.GenreId,
                                PreferredAuthor = model.PreferredAuthor
                            };

                            userPreferences.Add(userPreference);
                        }

                        _context.UserPreferences.AddRange(userPreferences);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        response.Success = true;
                        response.Data = models; 
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = "Failed to create UserPreferences. " + ex.Message;
                    }
                }
            });

            return response;
        }



        public async Task<ServiceResponse<List<UserPreferenceModel>>> SaveUserPreferences(string userId, List<int> genreIds)
        {
            var response = new ServiceResponse<List<UserPreferenceModel>>();
            var preferencesToAdd = genreIds.Select(genreId => new UserPreference
            {
                UserId = userId,
                GenreId = genreId,
                PreferredAuthor="test"
                
            }).ToList();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.UserPreferences.AddRange(preferencesToAdd);
                        await _context.SaveChangesAsync();

                        var preferencesModel = preferencesToAdd.Select(p => new UserPreferenceModel
                        {
                            UserId = p.UserId,
                            GenreId = p.GenreId,
                            PreferredAuthor = ""
                        }).ToList();

                        await transaction.CommitAsync();
                        response.Success = true;
                        response.Data = preferencesModel;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to create user preferences due to: {ex.Message}";
                    }
                }
            });

            return response;
        }




        public async Task<ServiceResponse<UserPreferenceModel>> Update(UserPreferenceModel model)
        {
            var response = new ServiceResponse<UserPreferenceModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var UserPreference = await _context.UserPreferences.FindAsync(model.Id);
            if (UserPreference == null)
            {
                response.Success = false;
                response.Message = "UserPreference not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        UserPreference.UserId = model.UserId;
                        UserPreference.GenreId = model.GenreId;
                        UserPreference.PreferredAuthor = model.PreferredAuthor;


                        _context.UserPreferences.Update(UserPreference);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update UserPreference: {ex.Message}";
                    }
                }
            });

            return response;
        }
    }
}
