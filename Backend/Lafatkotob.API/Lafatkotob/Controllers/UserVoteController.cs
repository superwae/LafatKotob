using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Lafatkotob.Services.UserVoteService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Lafatkotob.ViewModels;

namespace Lafatkotob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VotesController : ControllerBase
    {
        private readonly IUserVoteService _voteService;

        public VotesController(IUserVoteService voteService)
        {
            _voteService = voteService;
        }



        [HttpPost("vote")]
        public async Task<IActionResult> Vote([FromBody] VoteModel input)
        {
            // Get voterUserId from claims for security
            var voterUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (voterUserId == null || voterUserId != input.VoterUserId)
            {
                return Unauthorized("Invalid user ID.");
            }

            var result = input.IsUpvote
                ? await _voteService.AddUpvoteAsync(input.VoterUserId, input.TargetUserId)
                : await _voteService.AddDownvoteAsync(input.VoterUserId, input.TargetUserId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
    }
}
