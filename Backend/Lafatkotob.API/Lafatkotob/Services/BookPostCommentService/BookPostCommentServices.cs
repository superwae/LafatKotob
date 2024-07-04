using Lafatkotob.Entities;
using Lafatkotob.ViewModels;
using Microsoft.EntityFrameworkCore;
using static System.Reflection.Metadata.BlobBuilder;

namespace Lafatkotob.Services.BookPostCommentService
{
    public class BookPostCommentServices : IBookPostCommentServices
    {
        private readonly ApplicationDbContext _context;
        public BookPostCommentServices(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<BookPostCommentModel>> Post(BookPostCommentModel model)
        {
            var response = new ServiceResponse<BookPostCommentModel>();
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var BookPostComment = new BookPostComment
                        {
                            Id = model.Id,
                            CommentText = model.CommentText,
                            UserId = model.UserId,
                            DateCommented = DateTime.Now,
                            BookId = model.BookId,

                        };
                        _context.BookPostComments.Add(BookPostComment);
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
public async Task<BookPostCommentModel> GetById(int id)
        {
            var BookPostComment = await _context.BookPostComments.FindAsync(id);
            if (BookPostComment == null) return null;

            return new BookPostCommentModel
            {
                Id = BookPostComment.Id,
                CommentText = BookPostComment.CommentText,
                UserId = BookPostComment.UserId,
                DateCommented = BookPostComment.DateCommented,
                BookId = BookPostComment.BookId
            };
        }
        public async Task<List<BookPostCommentModel>> GetAll()
        {
            return await _context.BookPostComments
                .Select(up => new BookPostCommentModel
                {
                    Id = up.Id,
                    CommentText = up.CommentText,
                    UserId = up.UserId,
                    DateCommented = up.DateCommented,
                    BookId = up.BookId
                })
                .ToListAsync();
        }
        public async Task<ServiceResponse<BookPostCommentModel>> Update(BookPostCommentModel model)
        {
            var response = new ServiceResponse<BookPostCommentModel>();
            if (model == null)
            {
                response.Success = false;
                response.Message = "Model cannot be null.";
                return response;
            }
            var BookPostComment = await _context.BookPostComments.FindAsync(model.Id);
            if (BookPostComment == null)
            {

                response.Success = false;
                response.Message = "BookPostComment not found.";
                return response;
            }
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        BookPostComment.CommentText = model.CommentText;
                        BookPostComment.UserId = model.UserId;
                        BookPostComment.DateCommented = model.DateCommented;
                        BookPostComment.BookId = model.BookId;
                        _context.BookPostComments.Update(BookPostComment);
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

        public async Task<ServiceResponse<BookPostCommentModel>> Delete(int id)
        {
            var response = new ServiceResponse<BookPostCommentModel>();
            var BookPostComment = await _context.BookPostComments.FindAsync(id);
            if (BookPostComment == null){
                response.Success = false;
                response.Message = "BookPostComment not found";
                return response;
            }
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.BookPostComments.Remove(BookPostComment);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        response.Success = true;

                        response.Data = new BookPostCommentModel
                        {
                            Id = BookPostComment.Id,
                            CommentText = BookPostComment.CommentText,
                            UserId = BookPostComment.UserId,
                            DateCommented = BookPostComment.DateCommented,
                            BookId = BookPostComment.BookId
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
