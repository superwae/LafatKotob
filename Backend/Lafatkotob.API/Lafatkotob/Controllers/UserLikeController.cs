using Lafatkotob.Services.UserLikeService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserLikeController : Controller
    {
        private readonly IUserLikeService _userLikeService;
        public UserLikeController(IUserLikeService userLikeService)
        {
            _userLikeService = userLikeService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllUserLikes()
        {
            var userLikes = await _userLikeService.GetAll();
            if(userLikes == null) return BadRequest();
            return Ok(userLikes);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetUserLikeById(int userLikeId)
        {
            var userLike = await _userLikeService.GetById(userLikeId);
            if (userLike == null) return BadRequest();
            return Ok(userLike);
        }
        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostUserLike(UserLikeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userLikeService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteUserLike(int userLikeId)
        {
            var userLike = await _userLikeService.Delete(userLikeId);
            if (userLike == null) return BadRequest();
            return Ok(userLike);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUserLike(UserLikeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userLikeService.Update(model);
            return Ok();
        }
       
    }
}
