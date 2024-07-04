using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace Lafatkotob.Services.BadgeService
{
    public class BadgeService : IBadgeService
    {
        private readonly ApplicationDbContext _context;
        public BadgeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<BadgeModel>>> GetAllBadgesByUser(string userId)
        {
            var ServiceResponse = new ServiceResponse<List<BadgeModel>>();


            var Badges = await _context.UserBadges.Where(bu => bu.UserId == userId)
                .Include(bu => bu.Badge)
                .Select(bu => new BadgeModel
                {
                    BadgeName=bu.Badge.BadgeName,
                    Description=bu.Badge.Description,
                })
                .ToListAsync();
            if(Badges.Any())
            {
                ServiceResponse.Success = true;
                ServiceResponse.Data = Badges;

                return ServiceResponse;
            }

            ServiceResponse.Success = false;
            ServiceResponse.Message = "Faild to fetch badges";
            return ServiceResponse;
            }
               

        

        public async Task<ServiceResponse<BadgeModel>> Post(BadgeModel model)
        {
            var response = new ServiceResponse<BadgeModel>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        var badge = new Badge
                        {
                            Id = model.Id,
                            BadgeName = model.BadgeName,
                            Description = model.Description
                        };
                        _context.Badges.Add(badge);
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
                    }
                }
            });

            return response;
        }
       
        public async Task<BadgeModel> GetById(int id)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) return null;

            return new BadgeModel
            {
                Id = badge.Id,
                BadgeName = badge.BadgeName,
                Description = badge.Description
            };
            
        }
        public async Task<List<BadgeModel>> GetAll()
        {
            return await _context.Badges
                .Select(up => new BadgeModel
                {
                    Id = up.Id,
                    BadgeName = up.BadgeName,
                    Description = up.Description
                })
                .ToListAsync();
        }
        public async Task<ServiceResponse<BadgeModel>> Update(BadgeModel model)
        {
            var response = new ServiceResponse<BadgeModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var badge = await _context.Badges.FindAsync(model.Id);
            if (badge == null)
            {
                response.Success = false;
                response.Message = "Badge not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        badge.BadgeName = model.BadgeName;
                        badge.Description = model.Description;

                        _context.Badges.Update(badge);
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

        public async Task<ServiceResponse<BadgeModel>> Delete(int id)
        {
            var response = new ServiceResponse<BadgeModel>();

            var badge = await _context.Badges.FindAsync(id);
            if (badge == null)
            {
                response.Success = false;
                response.Message = "Badge not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Badges.Remove(badge);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new BadgeModel 
                        {
                            Id = badge.Id,
                            BadgeName = badge.BadgeName,
                            Description = badge.Description
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
