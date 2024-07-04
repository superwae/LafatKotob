using Lafatkotob.Entities;
using Lafatkotob.Services.HistoryService;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lafatkotob.Services.HistoryService
{
    public class HistoryService : IHistoryService
    {
        private readonly ApplicationDbContext _context;

        public HistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<int>> Post(string userId)
        {
            var response = new ServiceResponse<int>();

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        var History = new History
                        {
                            UserId = userId,
                            Date = DateTime.Now,

                        };

                       
                        _context.History.Add(History);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        response.Success = true;
                        response.Data = History.Id;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Success = false;
                        response.Message = "Failed to create History.";
                    }
                }
            });

            return response;
        }

        public async Task<HistoryModel> GetById(int id)
        {
            var history = await _context.History.FindAsync(id);
            if (history == null) return null;

            return new HistoryModel
            {
                Id = history.Id,
                UserId = history.UserId,
                Date = history.Date,

            };
        }

        public async Task<List<HistoryModel>> GetAll()
        {
            return await _context.History
                .Select(h => new HistoryModel
                {
                    Id = h.Id,
                    UserId = h.UserId,
                    Date = h.Date,

                })
                .ToListAsync();
        }

        public async Task<ServiceResponse<HistoryModel>> Update(HistoryModel model)
        {
            var response = new ServiceResponse<HistoryModel>();

            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }

            var History = await _context.History.FindAsync(model.Id);
            if (History == null)
            {
                response.Success = false;
                response.Message = "History not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        History.Date = model.Date;



                        _context.History.Update(History);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = model;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to update History: {ex.Message}";
                    }
                }
            });

            return response;
        }

        public async Task<ServiceResponse<HistoryModel>> Delete(int id)
        {
            var response = new ServiceResponse<HistoryModel>();

            var History = await _context.History.FindAsync(id);
            if (History == null)
            {
                response.Success = false;
                response.Message = "History not found.";
                return response;
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.History.Remove(History);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Success = true;
                        response.Data = new HistoryModel
                        {
                            Id = History.Id,
                            UserId = History.UserId,
                            Date = History.Date,

                        };
                       
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        response.Success = false;
                        response.Message = $"Failed to delete History: {ex.Message}";
                    }
                }
            });

            return response;
        }
    }
}
