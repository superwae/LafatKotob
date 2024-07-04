namespace Lafatkotob.Services.UserVoteService
{
    public interface IUserVoteService
    {
        Task<ServiceResponse<bool>> AddUpvoteAsync(string voterUserId, string targetUserId);
        Task<ServiceResponse<bool>> AddDownvoteAsync(string voterUserId, string targetUserId);

    }
}
