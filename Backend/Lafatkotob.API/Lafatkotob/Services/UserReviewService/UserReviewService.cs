using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.UserReviewService
{
    public class UserReviewService : IUserReviewService
    {
        private readonly ApplicationDbContext _context;

        public UserReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<UserReviewModel>> Delete(int id)
        {
            var response = new ServiceResponse<UserReviewModel>();

            var UserReview = await _context.UserReviews.FindAsync(id);
            if (UserReview == null)
            {
                response.Success = false;
                response.Message = "UserReview not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.UserReviews.Remove(UserReview);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new UserReviewModel
                        {
                            Id = UserReview.Id,
                            ReviewedUserId = UserReview.ReviewedUserId,
                            ReviewingUserId = UserReview.ReviewingUserId,
                            ReviewText = UserReview.ReviewText,
                            DateReviewed = UserReview.DateReviewed,
                            Rating = UserReview.Rating
                        };
                        
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete UserReview: {ex.Message}";
                    }
                }
            });

            return response;
        }

        public async Task<UserReviewModel> GetById(int id)
        {
            var userReview = await _context.UserReviews.FindAsync(id);
            if (userReview == null) return null;

            return new UserReviewModel
            {
                Id = userReview.Id,
                ReviewedUserId = userReview.ReviewedUserId,
                ReviewingUserId = userReview.ReviewingUserId,
                ReviewText = userReview.ReviewText,
                DateReviewed = userReview.DateReviewed,
                Rating = userReview.Rating
            };
        }

        public async Task<List<UserReviewModel>> GetAll()
        {
            return await _context.UserReviews
                .Select(ur => new UserReviewModel
                {
                    Id = ur.Id,
                    ReviewedUserId = ur.ReviewedUserId,
                    ReviewingUserId = ur.ReviewingUserId,
                    ReviewText = ur.ReviewText,
                    DateReviewed = ur.DateReviewed,
                    Rating = ur.Rating
                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<UserReviewModel>> Post(UserReviewModel model)
        {
            var response = new ServiceResponse<UserReviewModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        var UserReview = new UserReview
                        {
                            ReviewedUserId = model.ReviewedUserId,
                            ReviewingUserId = model.ReviewingUserId,
                            ReviewText = model.ReviewText,
                            DateReviewed = DateTime.Now,
                            Rating = model.Rating
                        };  
                        _context.UserReviews.Add(UserReview);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create UserReview.";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<UserReviewModel>> Update(UserReviewModel model)
        {
            var response = new ServiceResponse<UserReviewModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var UserReview = await _context.UserReviews.FindAsync(model.Id);
            if (UserReview == null)
            {
                response.Success = false;
                response.Message = "UserReview not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        UserReview.ReviewText = model.ReviewText;
                        UserReview.Rating = model.Rating;


                        _context.UserReviews.Update(UserReview);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update UserReview: {ex.Message}";
                    }
                }
            });

            return response;
        }
    }
}
