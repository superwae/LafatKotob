using Lafatkotob.Entities;
using Lafatkotob.Services.UserBadgeService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Lafatkotob.Services.UserVoteService
{
    public class UserVoteService : IUserVoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserBadgeService _userBadge;
        public UserVoteService(ApplicationDbContext context, IUserBadgeService userBadge, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _userBadge = userBadge;
        }

        public async Task<ServiceResponse<bool>> AddUpvoteAsync(string voterUserId, string targetUserId)
        {
            return await AddVoteAsync(voterUserId, targetUserId, true);
        }

        public async Task<ServiceResponse<bool>> AddDownvoteAsync(string voterUserId, string targetUserId)
        {
            return await AddVoteAsync(voterUserId, targetUserId, false);
        }

        private async Task<ServiceResponse<bool>> AddVoteAsync(string voterUserId, string targetUserId, bool isUpvote)
        {
            int minus = 0;
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var voterUser = await _userManager.FindByNameAsync(voterUserId);
                        var targetUser = await _userManager.FindByNameAsync(targetUserId);
                        voterUserId= voterUser.Id;
                        targetUserId = targetUser.Id;

                        if (voterUserId == targetUserId)
                        {
                            return new ServiceResponse<bool> { Success = false, Message = "Cannot vote for oneself." };
                        }

                        var existingVote = await _context.UserVotes
                            .FirstOrDefaultAsync(v => v.VoterUserId == voterUserId && v.TargetUserId == targetUserId);

                        if (existingVote != null)
                        {
                            if (existingVote.IsUpvote == isUpvote)
                            {
                                return new ServiceResponse<bool> { Success = false, Message = "No change needed." };
                            }

                            existingVote.IsUpvote = isUpvote;
                            if(isUpvote)
                            {
                                minus = 1;
                            }
                            else
                            {
                                minus = -1;
                            }
                            _context.UserVotes.Update(existingVote);
                        }
                        else
                        {
                            var vote = new UserVote
                            {
                                VoterUserId = voterUserId,
                                TargetUserId = targetUserId,
                                IsUpvote = isUpvote
                            };
                            _context.UserVotes.Add(vote);
                        }

                        await _context.SaveChangesAsync();

                        if (targetUser == null)
                        {
                            throw new Exception("Target user not found");
                        }

                        if (isUpvote)
                        {
                            targetUser.UpVotes = (targetUser.UpVotes ?? 0) + 1+minus;
                            if(targetUser.UpVotes >50)
                            {
                                var badge = await _context.Badges.Where(b => b.BadgeName == "ReviewGold")
                                .FirstOrDefaultAsync();
                                var userbadge = await _context.UserBadges.Where(us => us.BadgeId == badge.Id && us.UserId == targetUserId).FirstOrDefaultAsync();
                                if (userbadge == null)
                                {
                                     var userBadge = new UserBadgeModel
                                    {
                                        BadgeId = badge.Id,
                                        UserId = targetUserId,
                                        DateEarned = DateTime.Now

                                    };
                                    _userBadge.Post(userBadge);
                                    await _context.SaveChangesAsync();
                                }

                            }
                            else if (targetUser.UpVotes >20)
                            {
                                var badge = await _context.Badges.Where(b => b.BadgeName == "ReviewSilver")
                             .FirstOrDefaultAsync();
                                var userbadge = await _context.UserBadges.Where(us => us.BadgeId == badge.Id && us.UserId == targetUserId).FirstOrDefaultAsync();
                                if (userbadge == null)
                                {

                                    var userBadge = new UserBadgeModel
                                    {
                                        BadgeId = badge.Id,
                                        UserId = targetUserId,
                                        DateEarned = DateTime.Now

                                    };
                                    _userBadge.Post(userBadge);
                                    await _context.SaveChangesAsync();
                                }
                            }
                            else if (targetUser.UpVotes > 5)
                            {
                                var badgee = await _context.Badges.Where(b => b.BadgeName == "ReviewBronze")
                                .Select(b => new BadgeModel
                                {
                                    Id = b.Id,
                                    BadgeName = b.BadgeName,
                                    Description = b.Description

                                })
                                .FirstOrDefaultAsync();
                                var userbadgee = await _context.UserBadges.Where(us => us.BadgeId == badgee.Id && us.UserId == targetUserId)
                                 .Select(b => new UserBadgeModel
                                 {
                                     UserId=b.UserId,
                                     BadgeId=b.BadgeId,
                                     DateEarned=b.DateEarned,

                                 })
                                .FirstOrDefaultAsync();
                                if (userbadgee == null)
                                {
                                    var userBadge = new UserBadgeModel
                                    {
                                        BadgeId = badgee.Id,
                                        UserId = targetUserId,
                                        DateEarned = DateTime.Now

                                    };
                                    _userBadge.Post(userBadge);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            targetUser.UpVotes = (targetUser.UpVotes ?? 0) - 1+minus;
                        }

                        await _userManager.UpdateAsync(targetUser);

                        await transaction.CommitAsync();

                        return new ServiceResponse<bool> { Success = true, Message = "Vote updated successfully." };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return new ServiceResponse<bool> { Success = false, Message = ex.Message };
                    }
                }
            });
        }
    }
}
