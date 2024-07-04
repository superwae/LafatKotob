using Lafatkotob.Services.UserReviewService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserReviewController : Controller
    {
        private readonly IUserReviewService _userReviewService;
        public UserReviewController(IUserReviewService userReviewService)
        {
            _userReviewService = userReviewService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllUserReviews()
        {
            var userReviews = await _userReviewService.GetAll();
            if(userReviews == null) return BadRequest();
            return Ok(userReviews);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetUserReviewById(int userReviewId)
        {
            var userReview = await _userReviewService.GetById(userReviewId);
            if (userReview == null) return BadRequest();
            return Ok(userReview);
        }
        [HttpPost("post")]

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostUserReview(UserReviewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userReviewService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteUserReview(int userReviewId)
        {
            var userReview = await _userReviewService.Delete(userReviewId);
            if (userReview == null) return BadRequest();
            return Ok(userReview);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUserReview(UserReviewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userReviewService.Update(model);
            return Ok();
        }
       
    }
}
