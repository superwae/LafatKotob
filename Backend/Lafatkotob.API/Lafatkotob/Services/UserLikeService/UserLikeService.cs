using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.UserLikeService
{
    public class UserLikeService : IUserLikeService
    {
        private readonly ApplicationDbContext _context;

        public UserLikeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<UserLikeModel>> Delete(int id)
        {
            var response = new ServiceResponse<UserLikeModel>();

            var UserLike = await _context.UserLikes.FindAsync(id);
            if (UserLike == null)
            {
                response.Success = false;
                response.Message = "UserLike not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.UserLikes.Remove(UserLike);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new UserLikeModel
                        {
                            Id = UserLike.Id,
                            LikedUserId = UserLike.LikedUserId,
                            LikingUserId = UserLike.LikingUserId,
                            DateLiked = UserLike.DateLiked
                        };
                       
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete UserLike: {ex.Message}";
                    }
                }
            });

            return response;
        }
        public async Task<UserLikeModel> GetById(int id)
        {
            var userLike = await _context.UserLikes.FindAsync(id);
            if (userLike == null) return null;

            return new UserLikeModel
            {
                Id = userLike.Id,
                LikedUserId = userLike.LikedUserId,
                LikingUserId = userLike.LikingUserId,
                DateLiked = userLike.DateLiked
            };
        }

        public async Task<List<UserLikeModel>> GetAll()
        {
            return await _context.UserLikes
                .Select(ul => new UserLikeModel
                {
                    Id = ul.Id,
                    LikedUserId = ul.LikedUserId,
                    LikingUserId = ul.LikingUserId,
                    DateLiked = ul.DateLiked
                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<UserLikeModel>> Post(UserLikeModel model)
        {
            var response = new ServiceResponse<UserLikeModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        var UserLike = new UserLike
                        {
                            LikedUserId = model.LikedUserId,
                            LikingUserId = model.LikingUserId,
                            DateLiked = DateTime.Now
                        };
                       
                        _context.UserLikes.Add(UserLike);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create UserLike.";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<UserLikeModel>> Update(UserLikeModel model)
        {
            var response = new ServiceResponse<UserLikeModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var UserLike = await _context.UserLikes.FindAsync(model.Id);
            if (UserLike == null)
            {
                response.Success = false;
                response.Message = "UserLike not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        
                        UserLike.DateLiked = DateTime.Now;


                        _context.UserLikes.Update(UserLike);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update UserLike: {ex.Message}";
                    }
                }
            });

            return response;
        }
    }
}
